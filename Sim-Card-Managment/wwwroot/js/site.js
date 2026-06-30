// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', () => {

    Highcharts.chart('sim-usage-chart', {
        chart: {
            type: 'column',
            backgroundColor: '#F8F9FA', // Matching the off-white background in the image
            borderRadius: 16 // Rounds the outer container if desired
        },

        // 1. Left-aligned Typography
        title: {
            text: 'Weekly activity',
            align: 'left',
            margin: 10,
            style: {
                color: '#000000',
                fontWeight: 'bold',
                fontSize: '22px'
            }
        },
        subtitle: {
            text: 'Newly Assigned SIMs per day',
            align: 'left',
            style: {
                color: '#888888',
                fontSize: '15px'
            }
        },

        // 2. Clean X-Axis (No bold lines or tick marks)
        xAxis: {
            categories: ['Sat', 'Sun', 'Mon', 'Tue', 'Wed'],
            lineWidth: 0, // Removes the solid horizontal axis line
            tickWidth: 0, // Removes the little vertical tick marks
            labels: {
                style: {
                    color: '#6c757d',
                    fontWeight: 'bold',
                    fontSize: '13px'
                }
            }
        },

        // 3. Subtle Y-Axis
        yAxis: {
            title: {
                text: null // Hides the side title completely
            },
            min: 0,
            max: 100, // Forces the chart to scale to 100
            tickInterval: 20, // Creates lines at 0, 20, 40, 60, 80, 100
            gridLineColor: '#EAEAEA', // Very light gray horizontal lines
            labels: {
                style: {
                    color: '#6c757d',
                    fontSize: '13px'
                }
            }
        },

        // 4. Hide the Legend (since the design doesn't have one)
        legend: {
            enabled: false
        },

        // 5. Column Styling (The pill shape)
        plotOptions: {
            column: {
                borderRadius: 8, // Gives the columns that smooth rounded pill look
                borderWidth: 0, // Removes default borders
                pointPadding: 0.1, // Adjusts spacing between the two bars in a group
                groupPadding: 0.3  // Adjusts spacing between the days
            }
        },

        // 6. The Data (Two series for the two bars)
        series: [
            {
                name: 'Assigned',
                // Data for [Sat, Sun, Mon, Tue, Wed]
                data: [100, 0, 0, 0, 0],
                color: '#E24D17' // The dark FATHALL brand orange
            },
            {
                name: 'Pending',
                // Data for [Sat, Sun, Mon, Tue, Wed]
                data: [45, 0, 0, 0, 0],
                color: '#E08563' // A lighter, softer peach/orange
            }
        ],

        // Optional: Clean up the tooltip to match the minimal vibe
        credits: {
            enabled: false // Removes the "Highcharts.com" watermark
        }
    });

});