﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkyWorZonesCreation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyWorZonesCreation.Controllers
{
    interface IWorkZone
    {
        bool ExistAsync(string workZone);

        ResponseOFSC Add(string resource, string workZone);

        List<string> Ranges(WorkZone workZone);

        ResponseOFSC Create(WorkZone workZone);

        ResponseOFSC Exist(WorkZone workZone);

        ResponseOFSC Set(WorkZone workZone);


    }
    public class WorkZoneController : IWorkZone
    {
        public ResponseOFSC Add(string resource, string workZone)
        {
            DateTime dateTimeStart = DateTime.Now;
            DateTime dateTimeEnd = dateTimeStart.AddYears(1);
            Console.WriteLine(dateTimeStart.ToString("yyyy-MM-dd"));
            dynamic objResource = new JObject();
            objResource.workZone = workZone;
            objResource.startDate = dateTimeStart.ToString("yyyy-MM-dd");
            objResource.endDate = dateTimeEnd.ToString("yyyy-MM-dd");
            objResource.ratio = 100;
            objResource.recurrence = "";


            ResponseOFSC result = UtilWebRequest.SendWayAsync("rest/ofscCore/v1/resources/" + resource + "/workZones",
                                                               enumMethod.PUT,
                                                               objResource.ToString(Formatting.None));

            if (result.statusCode >= 200 && result.statusCode <= 400)
                result.flag = true;
            else
            {
                // /Program.Logger(string.Format("Bad Recurso:{0}|Zona Trabajo:{1}|Contenido:{2}|Mensaje:{3}", resource, workZone, result.Content, result.ErrorMessage), 2);
                result.flag = false;
            }
            return result;
        }

        public ResponseOFSC Create(WorkZone workZone)
        {
            ResponseOFSC result = new ResponseOFSC();
            RootWorkZone rootWorkZone = new RootWorkZone();
            rootWorkZone.items = new List<WorkZone>();
            WorkZone workZoneItem = new WorkZone();

            // start 
            if (workZone.travelArea != "MX")
                workZoneItem.workZoneLabel = workZone.keylabel.FirstOrDefault();
            else
                workZoneItem.workZoneLabel = workZone.workZoneLabel;

            workZoneItem.status = workZone.status;
            workZoneItem.travelArea = workZone.travelArea;
            workZoneItem.workZoneName = workZone.workZoneName;
            workZoneItem.keys.Add(workZone.keylabel.FirstOrDefault());
            rootWorkZone.items.Add(workZoneItem);

            // end

            result = UtilWebRequest.SendWayAsync("rest/ofscMetadata/v1/workZones",
                                                      enumMethod.POST,
                                                      JsonConvert.SerializeObject(workZoneItem, Formatting.Indented));

            if (result.statusCode >= 200 && result.statusCode <= 300)
            {
                result.flag = true;
                return result;
            }
            // exist current workzone then set info workzone
            if (result.statusCode == 409)
            {
                result = UtilWebRequest.SendWayAsync("rest/ofscMetadata/v1/workZones/" + workZone.workZoneLabel,
                                                   enumMethod.PUT,
                                                   JsonConvert.SerializeObject(workZoneItem, Formatting.Indented));
                if (result.statusCode == 200 || result.statusCode == 201)
                    result.flag = true;
                else
                    result.flag = false;

                Program.Logger(result.ErrorMessage , 2);

            }
            else
            {
                // Program.Logger(string.Format("Bad Zona Trabajo:{0}|Contenido:{1}|Mensaje:{2}", workZone.workZoneName + "&" + workZone.keylabel, result.Content, result.ErrorMessage), 2);
                result.flag = false;
                Program.Logger(result.ErrorMessage, 2);

            }
            return result;
        }

        public ResponseOFSC Exist(WorkZone workZone)
        {

            // check exist
            ResponseOFSC result = UtilWebRequest.SendWayAsync("rest/ofscMetadata/v1/workZones/" + workZone.keylabel.FirstOrDefault(),
                                        enumMethod.GET,
                                        string.Empty);

            //ResponseOFSC result = UtilWebRequest.SendWayAsync("rest/ofscMetadata/v1/workZones/" + workZone.workZoneLabel,
            //                            enumMethod.GET,
            //                            string.Empty);

            if (result.statusCode >= 200 && result.statusCode < 300)
                result.flag = true;
            else
                result.flag = false;
            return result;
        }

        public bool ExistAsync(string zipCode)
        {
            bool flag = false;
            try
            {
                ResponseOFSC result = UtilWebRequest.SendWayAsync(string.Format("https://api-codigos-postales.herokuapp.com/v2/codigo_postal/{0}", zipCode), enumMethod.GET, "");
                dynamic results = JsonConvert.DeserializeObject<dynamic>(result.Content);
                var municipio = results.municipio;

                if (string.IsNullOrEmpty(municipio.Value))
                    flag = false;
                else
                    flag = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return flag;
        }

        public List<string> Ranges(WorkZone workZone)
        {
            workZone.id = new List<string>();
            try
            {
                string[] aWorkZonetmp = workZone.source.Split('|');

                foreach (var item in aWorkZonetmp)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;
                    var restmp = item.Split(',');

                    if (restmp.Count() > 1)
                    {
                        string[] aZipCode = restmp[1].Split('-');
                        if (aZipCode.Count() > 0)
                        {
                            int major = int.Parse(aZipCode[1]);
                            int minor = int.Parse(aZipCode[0]);

                            for (int i = minor; i <= major; i++)
                            {
                                string tmpzipcode = i.ToString().PadLeft(5, '0');
                                workZone.id.Add(tmpzipcode);
                            }
                        }
                    }
                    else
                    {
                        // TODO  hace falta información 
                    }
                }
                List<string> tmpB = new List<string>();
                tmpB.AddRange(workZone.id.Distinct());
                workZone.id.Clear();
                workZone.id.AddRange(tmpB);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception(ex.Message + " " + ex.InnerException);
            }

            return workZone.id;
        }

        public ResponseOFSC Set(WorkZone workZone)
        {

            RootWorkZone rootWorkZone = new RootWorkZone();
            WorkZone workZoneItem = new WorkZone();

            rootWorkZone.items = new List<WorkZone>();
            if (workZone.travelArea != "MX")
                workZoneItem.workZoneLabel = workZone.keylabel.FirstOrDefault();
            else
                workZoneItem.workZoneLabel = workZone.workZoneLabel;

            workZoneItem.status = workZone.status;
            workZoneItem.travelArea = workZone.travelArea;
            workZoneItem.workZoneName = workZone.workZoneName;
            workZoneItem.keys.Add(workZone.keylabel.FirstOrDefault());
            rootWorkZone.items.Add(workZoneItem);

            ResponseOFSC result = UtilWebRequest.SendWayAsync("rest/ofscMetadata/v1/workZones",
                                     enumMethod.PATCH,
                                     JsonConvert.SerializeObject(rootWorkZone, Formatting.Indented));
      
            if (result.statusCode >= 200 && result.statusCode < 300)
                result.flag = true;
            else
                result.flag = false;

            return result;
        }
    }
}
