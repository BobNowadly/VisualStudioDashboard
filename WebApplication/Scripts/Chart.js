﻿function DateChartFactory (model) {
    this.chartModel = model;
}

DateChartFactory.prototype.create = function (chartSelector) {
    // Wrapping in nv.addGraph allows for '0 timeout render', stores rendered charts in nv.graphs, and may do more in the future... it's NOT required
    var chart;
    var yAxisName = this.chartModel.yAxisLabel;
    var data = this.chartModel.data;

    nv.addGraph(function () {
        chart = nv.models.lineChart()
            .options({
                margin: { left: 100, bottom: 100 },
                showXAxis: true,
                showYAxis: true,
                transitionDuration: 250
            });

        // chart sub-models (ie. xAxis, yAxis, etc) when accessed directly, return themselves, not the parent chart, so need to chain separately
        chart.xAxis
            .axisLabel("Date")
            .rotateLabels(-45)
            .tickFormat(function (d) {
                return d3.time.format('%b %d')(new Date(d));
            });

        chart.yAxis
            .axisLabel(yAxisName);

        d3.select(chartSelector)
            .datum(data)
            .call(chart);

        //TODO: Figure out a good way to do this automatically
        nv.utils.windowResize(chart.update);
        //nv.utils.windowResize(function() { d3.select('#chart1 svg').call(chart) });

        chart.dispatch.on('stateChange', function (e) { nv.log('New State:', JSON.stringify(e)); });

        return chart;
    });
};

(function ($) {

    $.fn.doSpin = function () {
        var opts = {
            lines: 13, // The number of lines to draw
            length: 20, // The length of each line
            width: 10, // The line thickness
            radius: 30, // The radius of the inner circle
            corners: 1, // Corner roundness (0..1)
            rotate: 0, // The rotation offset
            direction: 1, // 1: clockwise, -1: counterclockwise
            color: '#000', // #rgb or #rrggbb or array of colors
            speed: 1, // Rounds per second
            trail: 60, // Afterglow percentage
            shadow: false, // Whether to render a shadow
            hwaccel: false, // Whether to use hardware acceleration
            className: 'spinner', // The CSS class to assign to the spinner
            zIndex: 2e9, // The z-index (defaults to 2000000000)
            top: '50%', // Top position relative to parent
            left: '50%' // Left position relative to parent
        };
        var target = this.get();
        return new Spinner(opts).spin(target[0]);
    };  
}(jQuery));