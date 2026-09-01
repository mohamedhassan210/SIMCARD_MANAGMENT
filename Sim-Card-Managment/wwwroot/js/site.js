document.addEventListener('DOMContentLoaded', () => {
    const chartContainer = document.getElementById('sim-usage-chart');
    if (!chartContainer) return;

    // Fetch the live database array metrics from our controller endpoint
    fetch(window.appRoot + 'Home/GetWeeklyActivityData')
        .then(response => response.json())
        .then(liveData => {

            Highcharts.chart('sim-usage-chart', {
                chart: {
                    type: 'column',
                    backgroundColor: '#fff',
                    borderRadius: 16
                },
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
                xAxis: {
                    categories: ['Sat', 'Sun', 'Mon', 'Tue', 'Wed','Thu'],
                    lineWidth: 0,
                    tickWidth: 0,
                    labels: {
                        style: {
                            color: '#6c757d',
                            fontWeight: 'bold',
                            fontSize: '13px'
                        }
                    }
                },
                yAxis: {
                    title: {
                        text: null
                    },
                    min: 0,
                    allowDecimals: false, // <-- This forces 0, 1, 2, 3 instead of decimals!
                    gridLineColor: '#EAEAEA',
                    labels: {
                        style: {
                            color: '#6c757d',
                            fontSize: '13px'
                        }
                    }
                },
                legend: {
                    enabled: false
                },
                plotOptions: {
                    column: {
                        borderRadius: 8,
                        borderWidth: 0,
                        pointPadding: 0.1,
                        groupPadding: 0.3
                    }
                },
                series: [
                    {
                        name: 'SIM',
                        data: liveData.simData,
                        color: '#E24D17'
                    },
                    {
                        name: 'USB',
                        data: liveData.usbData,
                        color: '#E08563'
                    }
                ],
                credits: {
                    enabled: false
                }
            });
        })
        .catch(error => console.error('Error fetching dashboard chart numbers:', error));
});