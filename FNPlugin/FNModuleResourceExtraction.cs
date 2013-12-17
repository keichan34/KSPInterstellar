﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FNPlugin {
    class FNModuleResourceExtraction : FNResourceSuppliableModule{
        //Persistent True
        [KSPField(isPersistant = true)]
        public bool IsEnabled = false;

        // Persistent False
        [KSPField(isPersistant = false)]
        public float powerConsumptionLand;
        [KSPField(isPersistant = false)]
        public float powerConsumptionOcean;
        [KSPField(isPersistant = false)]
        public float extractionRateLandPerTon;
        [KSPField(isPersistant = false)]
        public float extractionRateOceanPerTon;
        [KSPField(isPersistant = false)]
        public string resourceName;
        [KSPField(isPersistant = false)]
        public string unitName;
        [KSPField(isPersistant = false)]
        public string extractActionName;
        [KSPField(isPersistant = false)]
        public string stopActionName;

        //GUI
        [KSPField(isPersistant = false, guiActive = true, guiName = "Status")]
        public string statusTitle;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Power")]
        public string powerStr;
        [KSPField(isPersistant = false, guiActive = true, guiName = "S")]
        public string resourceRate;

        //Internal
        double electrical_power_ratio = 0;
        double extraction_rate_d = 0;

        [KSPEvent(guiActive = true, guiName = "Start Action", active = true)]
        public void startResourceExtraction() {
            IsEnabled = true;
        }

        [KSPEvent(guiActive = true, guiName = "Stop Action", active = true)]
        public void stopResourceExtration() {
            IsEnabled = false;
        }
                

        public override void OnStart(PartModule.StartState state) {
            Events["startResourceExtraction"].guiName = extractActionName;
            Events["stopResourceExtration"].guiName = stopActionName;
            Fields["statusTitle"].guiName = unitName;
            if (state == StartState.Editor) { return; }
            part.force_activate();
        }

        public override void OnUpdate() {
            double resource_abundance = 0;
            bool resource_available = false;
            if (vessel.Landed) {
                FNPlanetaryResourcePixel current_resource_abundance_pixel = FNPlanetaryResourceMapData.getResourceAvailabilityByRealResourceName(vessel.mainBody.flightGlobalsIndex, resourceName, vessel.latitude, vessel.longitude);
                resource_abundance = current_resource_abundance_pixel.getAmount();
            } else if (vessel.Splashed) {
                resource_abundance = OceanicResourceHandler.getOceanicResourceContent(vessel.mainBody.flightGlobalsIndex, resourceName);
            }
            if (resource_abundance > 0) {
                resource_available = true;
            }
            Events["startResourceExtraction"].active = !IsEnabled && resource_available;
            Events["stopResourceExtration"].active = IsEnabled;
            if (IsEnabled) {
                Fields["powerStr"].guiActive = true;
                Fields["resourceRate"].guiActive = true;
                statusTitle = "Active";
                double power_required = 0;
                if (vessel.Landed) {
                    power_required = powerConsumptionLand;
                }else if(vessel.Splashed) {
                    power_required = powerConsumptionOcean;
                }
                powerStr = (power_required*electrical_power_ratio).ToString("0.000") + " MW / " + power_required.ToString("0.000") + " MW";
                double resource_density = PartResourceLibrary.Instance.GetDefinition(resourceName).density;
                double resource_rate_per_hour = extraction_rate_d * resource_density * 3600;
                resourceRate = formatMassStr(resource_rate_per_hour);
            } else {
                Fields["powerStr"].guiActive = false;
                Fields["resourceRate"].guiActive = false;
                statusTitle = "Offline";
            }
        }

        public override void OnFixedUpdate() {
            if (IsEnabled) {
                double power_requirements = 0;
                double extraction_time = 0;
                if (vessel.Landed) {
                    power_requirements = powerConsumptionLand;
                    extraction_time = extractionRateLandPerTon;
                } else if (vessel.Splashed) {
                    power_requirements = powerConsumptionOcean;
                    extraction_time = extractionRateOceanPerTon;
                } else {
                    IsEnabled = false;
                    return;
                }

                double electrical_power_provided = consumeFNResource(power_requirements * TimeWarp.fixedDeltaTime, FNResourceManager.FNRESOURCE_MEGAJOULES);
                if (power_requirements > 0) {
                    electrical_power_ratio = electrical_power_provided / TimeWarp.fixedDeltaTime / power_requirements;
                } else {
                    if (power_requirements < 0) {
                        IsEnabled = false;
                        return;
                    } else {
                        electrical_power_ratio = 1;
                    }
                }
                double resource_abundance = 0;
                if (vessel.Landed) {
                    FNPlanetaryResourcePixel current_resource_abundance_pixel = FNPlanetaryResourceMapData.getResourceAvailabilityByRealResourceName(vessel.mainBody.flightGlobalsIndex, resourceName, vessel.latitude, vessel.longitude);
                    resource_abundance = current_resource_abundance_pixel.getAmount();
                } else if (vessel.Splashed) {
                    resource_abundance = OceanicResourceHandler.getOceanicResourceContent(vessel.mainBody.flightGlobalsIndex, resourceName);
                }
                double extraction_rate = resource_abundance * extraction_time * electrical_power_ratio;
                if (resource_abundance > 0) {
                    double resource_density = PartResourceLibrary.Instance.GetDefinition(resourceName).density;
                    extraction_rate_d = -part.RequestResource(resourceName, -extraction_rate / resource_density * TimeWarp.fixedDeltaTime) / TimeWarp.fixedDeltaTime;
                } else {
                    IsEnabled = false;
                }
            }
        }

        public override string getResourceManagerDisplayName() {
            return unitName;
        }

        protected string formatMassStr(double mass) {
            if (mass > 1) {
                return mass.ToString("0.000") + " mT/hour";
            } else {
                if (mass > 0.001) {
                    return (mass*1000).ToString("0.000") + " kg/hour";
                }else{
                    if (mass > 1e-6) {
                        return (mass * 1e6).ToString("0.000") + " g/hour";
                    } else {
                        if(mass > 1e-9) {
                            return (mass * 1e9).ToString("0.000") + " mg/hour";
                        }else{
                            return (mass * 1e12).ToString("0.000") + " ug/hour";
                        }
                    }
                }
            }
        }

    }
}
