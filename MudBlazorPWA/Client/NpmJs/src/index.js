// Module Manager for registering the modules of the chart
import { ModuleManager } from 'igniteui-webcomponents-core';
// Radial Gauge Module
import { IgcRadialGaugeModule } from 'igniteui-webcomponents-gauges';

// register the modules
ModuleManager.register(
    IgcRadialGaugeModule
);

window.updateValue = function (value) {
    document.getElementById("rg").value = value;
}