using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace SmappeeCore
{
    /// <summary>
    /// Client for the local LAN API of the device
    /// </summary>
    public class SmappeeExpertClient : ISmappeeExpertClient
    {
        private ILogger<SmappeeExpertClient> _logger;

        private SmappeeExpertConfiguration _configuration;

        private HttpClient _httpClient;

        private bool _loggedIn = false;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        public SmappeeExpertClient(ILogger<SmappeeExpertClient> logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Login into the expert mode interface
        /// </summary>
        /// <param name="configuration"></param>
        public bool Login(SmappeeExpertConfiguration configuration)
        {
            _configuration = configuration;

            try
            {
                _httpClient = new HttpClient();

                //The Login password is passed as CONTENT for that request - weird practice..  but this is what is it!
                var logonHttpContent = new System.Net.Http.StringContent(_configuration.LoginPassword);

                logonHttpContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");

                _logger.LogInformation("Calling logon on Smappee");

                Uri loginUri = new Uri(string.Format("http://{0}:{1}/gateway/apipublic/logon", _configuration.SmappeLocalAddress,
                    _configuration.Port), UriKind.Absolute);

                var logonResponse = _httpClient.PostAsync( loginUri, logonHttpContent).Result;

                if (logonResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Smappee logon succeded");
                    _loggedIn = true;
                    return true;
                }
                else
                {
                    //no way to login?!?!
                    _logger.LogError(string.Format("Smappee logon failed.\nHTTP Status Code: {0}", logonResponse.StatusCode));

                    var loginStringResponse = logonResponse.Content.ReadAsStringAsync().Result;

                    _logger.LogError(string.Format("Login Response content:\n{0}", loginStringResponse));
                    return false;
                }
            }
            catch (Exception ex)
            {
                //someething wrong with the HTTP processing or other..
                _logger.LogError(string.Format("Something wrong with the connection to Smappee.\n{0}", ex.ToString()));
                return false;
            }
        }
        
        /// <summary>
        /// Get Instant Value
        /// </summary>
        /// <returns></returns>
        public List<SmappeeKeyValuePairs> GetInstantValue()
        {
            if (!_loggedIn)
            {
                _logger.LogError("Please Login first!");
                return null;
            }
            
            List<SmappeeKeyValuePairs> instantValues = null;

            _logger.LogInformation("Getting Instantaneouse values from Smappee");

            //create the request content for the instantaneous data
            var loadInstantaneousHttpContent = new System.Net.Http.StringContent("loadInstantaneous");
            loadInstantaneousHttpContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");

            Uri instantaneousUri = new Uri(string.Format("http://{0}:{1}/gateway/apipublic/instantaneous", _configuration.SmappeLocalAddress,
                _configuration.Port), UriKind.Absolute);

            var instantDataResponse = _httpClient.PostAsync(instantaneousUri, loadInstantaneousHttpContent).Result;

            if (instantDataResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Smappee instantaneous values received");
                
                var instantDataResponseString = instantDataResponse.Content.ReadAsStringAsync().Result;

                _logger.LogInformation(string.Format("Deserializing JSON:\n{0}", instantDataResponseString));

                try
                {
                    instantValues = JsonConvert.DeserializeObject<List<SmappeeKeyValuePairs>>(instantDataResponseString);
                }
                catch (Exception ex)
                {
                    _logger.LogError(string.Format("Error deserializing Smappee Instant data Details JSON.\n\n{0}\n\n{1}",
                        instantDataResponseString, ex));
                    
                    return null;
                }

                //[{"value":"1","key":"meterReaderRestart"},
                //{"value":"2057781","key":"phase0ActivePower"},
                //{"value":"99","key":"phase0Cosfi"},
                //{"value":"0","key":"phase0Quadrant"},
                //{"value":"0","key":"autoCommissioningCandidate"},
                //{"value":"0","key":"autoCommissioningRunning"},
                //{"value":"0","key":"autoCommissioningValidation"},
                //{"value":"0","key":"voltageReversed"}]
            }
            else
            {
                //SOME ISSUE happened during the Smappee api call   TODO also login timeout consideration

                _logger.LogError(string.Format("Smappee instantaneous receiving failed.\nHTTP Status Code: {0}", instantDataResponse.StatusCode));

                var instantDataResponseString = instantDataResponse.Content.ReadAsStringAsync().Result;

                _logger.LogError(string.Format("Instantaneous Response content:\n{0}", instantDataResponseString));
            }

            return instantValues;
        }
        
        /// <summary>
        /// Get Instant Value
        /// </summary>
        /// <returns></returns>
        public List<SmappeeKeyValuePairs> GetReportValue()
        {
            if (!_loggedIn)
            {
                _logger.LogError("Please Login first!");
                return null;
            }

            List<SmappeeKeyValuePairs> instantValues = new List<SmappeeKeyValuePairs>();

            _logger.LogInformation("Getting Report values from Smappee");

            //create the request content for the instantaneous data
            Uri instantaneousUri = new Uri(string.Format("http://{0}:{1}/gateway/apipublic/reportInstantaneousValues", _configuration.SmappeLocalAddress,
                _configuration.Port), UriKind.Absolute);

            var instantDataResponse = _httpClient.GetAsync(instantaneousUri).Result;

            if (instantDataResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Smappee report values received");

                var instantDataResponseString = instantDataResponse.Content.ReadAsStringAsync().Result;

                _logger.LogInformation(string.Format("Deserializing HTML STUFF:\n{0}", instantDataResponseString));

                //{ "report":"Instantaneous values:<BR>voltage=225.6 Vrms<BR>FFTComponents:<BR>Phase 1:<BR>\tcurrent=1.852 A,
                //activePower =373.433 W, reactivePower=187.861 var, apparentPower=418.024 VA, cosfi=89, quadrant=0,
                //phaseshift =0.0, phaseDiff=0.0<BR>\tFFTComponents:<BR><BR>
                //<BR>Phase 1, peak active power 4619.538 W at 14/11/2017 08:28:20
                //<BR>active energy RMS per phase mapping combination<BR>phase mapping -1=0.0 kWh [* 1/0]
                //<BR>phase mapping -1=0.0 kWh [ 1/0]<BR>phase mapping -1=0.0 kWh [ 1/0]
                //<BR>phase mapping -1=0.0 kWh [ 1/0]<BR>phase mapping -1=0.0 kWh [ 1/0]
                //<BR>phase mapping -1=0.0 kWh [ 1/0]<BR><BR>active energy RMS (solar) per phase mapping combination
                //<BR>phase mapping -1=0.0 kWh [* 1/0]<BR>phase mapping -1=0.0 kWh [ 1/0]<BR>phase mapping -1=0.0 kWh [ 1/0]
                //<BR>phase mapping -1=0.0 kWh [ 1/0]<BR>phase mapping -1=0.0 kWh [ 1/0]<BR>phase mapping -1=0.0 kWh [ 1/0]<BR><BR>"}

                try
                {
                    var indexOfVoltage = instantDataResponseString.IndexOf("voltage=");

                    var indexOfEndVoltage = instantDataResponseString.IndexOf(" Vrms");

                    var voltage = instantDataResponseString.Substring(indexOfVoltage, indexOfEndVoltage - indexOfVoltage).Remove(0,8);

                    instantValues.Add(new SmappeeKeyValuePairs() { key = SmappeeValueEnum.Voltage, value = voltage });
                    

                    var indexCurrent = instantDataResponseString.IndexOf("current =");
                    var indexCurrentEnd = instantDataResponseString.IndexOf(" A, activePower=");

                    var current = instantDataResponseString.Substring(indexCurrent, indexCurrentEnd - indexCurrent).Remove(0, 9);

                    instantValues.Add(new SmappeeKeyValuePairs() { key = SmappeeValueEnum.Current, value = current });

      
                    var indexActivePowerEnd = instantDataResponseString.IndexOf(" W, reactivePowe");

                    var activePower = instantDataResponseString.Substring(indexCurrentEnd, indexActivePowerEnd - indexCurrentEnd).Remove(0, 16);

                    instantValues.Add(new SmappeeKeyValuePairs() { key = SmappeeValueEnum.ActivePower, value = activePower });


                    var indexReactivePowerEnd = instantDataResponseString.IndexOf(" var, apparentPower=");

                    var reactivePower = instantDataResponseString.Substring(indexActivePowerEnd, indexReactivePowerEnd - indexActivePowerEnd).Remove(0, 16);

                    instantValues.Add(new SmappeeKeyValuePairs() { key = SmappeeValueEnum.ReactivePower, value = reactivePower });


                    var indexApparentPowerEnd = instantDataResponseString.IndexOf(", cosfi=");

                    var apparentPower = instantDataResponseString.Substring(indexReactivePowerEnd, indexApparentPowerEnd - indexReactivePowerEnd).Remove(0, 20);

                    instantValues.Add(new SmappeeKeyValuePairs() { key = SmappeeValueEnum.ApparentPower, value = apparentPower });



                    var indexPeakPower = instantDataResponseString.IndexOf("peak active power");
                    var indexPeakPowerEnd = instantDataResponseString.IndexOf(" W at ");

                    var peakPower = instantDataResponseString.Substring(indexPeakPower, indexPeakPowerEnd - indexPeakPower).Remove(0, 17);

                    instantValues.Add(new SmappeeKeyValuePairs() { key = SmappeeValueEnum.PeakValue, value = peakPower });

                    var timePeakEnd = instantDataResponseString.IndexOf("<BR>active energy RMS");

                    var peakTime = instantDataResponseString.Substring(indexPeakPowerEnd, timePeakEnd - indexPeakPowerEnd).Remove(0, 6);

                    instantValues.Add(new SmappeeKeyValuePairs() { key =  SmappeeValueEnum.PeakTime, value = peakTime });
                }
                catch (Exception ex)
                {
                    _logger.LogError(string.Format("Error deserializing Smappee Report data Details.\n\n{0}\n\n{1}",
                        instantDataResponseString, ex));

                    return null;
                }
            }
            else
            {
                //SOME ISSUE happened during the Smappee api call   TODO also login timeout consideration

                _logger.LogError(string.Format("Smappee instantaneous receiving failed.\nHTTP Status Code: {0}", instantDataResponse.StatusCode));

                var instantDataResponseString = instantDataResponse.Content.ReadAsStringAsync().Result;

                _logger.LogError(string.Format("Instantaneous Response content:\n{0}", instantDataResponseString));
            }

            return instantValues;
        }

        /// <summary>
        /// Reset Peak Value
        /// </summary>
        /// <returns></returns>
        public bool ResetPeakValue()
        {
            if (!_loggedIn)
            {
                _logger.LogError("Please Login first!");
                return null;
            }

            List<SmappeeKeyValuePairs> instantValues = new List<SmappeeKeyValuePairs>();

            _logger.LogInformation("Resetting Peak Value in Smappee");

            //create the request content for the instantaneous data
            Uri instantaneousUri = new Uri(string.Format("http://{0}:{1}/gateway/apipublic/resetActivePowerPeaks", _configuration.SmappeLocalAddress,
                _configuration.Port), UriKind.Absolute);

            //create the request content for the instantaneous data
            var justEmptyHttpContent = new System.Net.Http.StringContent(string.Empty);
            justEmptyHttpContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");


            var instantDataResponse = _httpClient.PostAsync(instantaneousUri, justEmptyHttpContent).Result;

            if (instantDataResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Peak Data resetted!");

                return true;
            }
            else
            {
                //SOME ISSUE happened during the Smappee api call   TODO also login timeout consideration

                _logger.LogError(string.Format("Peak Data NOT resetted.\nHTTP Status Code: {0}", instantDataResponse.StatusCode));

                var instantDataResponseString = instantDataResponse.Content.ReadAsStringAsync().Result;

                _logger.LogError(string.Format("Response content:\n{0}", instantDataResponseString));
            }

            return false;
        }

    }
}
