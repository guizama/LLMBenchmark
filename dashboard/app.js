async function loadData() {

    const benchmarkResultsRaw =
        await fetch('benchmark-results.json')
            .then(r => r.json());

    const validatorsRaw =
        await fetch('validators-breakdown.json')
            .then(r => r.json());

    const runsRaw =
        await fetch('benchmark-runs.json')
            .then(r => r.json());

    // Suporta formato:
    // {
    //   "SELECT ...": [ ... ]
    // }

    const benchmarkResults =
        extractArray(benchmarkResultsRaw);

    const validators =
        extractArray(validatorsRaw);

    const runs =
        extractArray(runsRaw);

    initializeDashboard(
        benchmarkResults,
        validators,
        runs
    );
}

function extractArray(obj) {

    if (Array.isArray(obj))
        return obj;

    const firstKey =
        Object.keys(obj)[0];

    return obj[firstKey] || [];
}

function initializeDashboard(results, validators, runs) {

    const latestRun = runs[0];

    if (latestRun) {

        document.getElementById('runStatus').innerText =
            `${latestRun.Status} · ${latestRun.TotalExecutions} executions`;
    }

    renderCards(results);

    renderCharts(results, validators);

    renderInsights(results);

    renderTable(results);
}

function renderCards(results) {

    const grouped = groupByModel(results);

    let bestModel = null;
    let bestScore = -1;

    let fastestModel = null;
    let fastestLatency = Number.MAX_VALUE;

    let efficientModel = null;
    let bestEfficiency = -1;

    let totalTokens = 0;

    for (const model in grouped) {

        const items = grouped[model];

        const avgScore =
            average(items.map(x => Number(x.JudgeScore) || 0));

        const avgLatency =
            average(items.map(x => Number(x.EndToEndLatencyMs) || 0));

        const avgEfficiency =
            average(items.map(x => Number(x.ScorePerToken) || 0));

        const tokens =
            sum(items.map(x => Number(x.TotalTokens) || 0));

        totalTokens += tokens;

        if (avgScore > bestScore) {
            bestScore = avgScore;
            bestModel = model;
        }

        if (avgLatency < fastestLatency) {
            fastestLatency = avgLatency;
            fastestModel = model;
        }

        if (avgEfficiency > bestEfficiency) {
            bestEfficiency = avgEfficiency;
            efficientModel = model;
        }
    }

    document.getElementById('bestModel').innerText =
        bestModel || '-';

    document.getElementById('bestModelScore').innerText =
        `Score ${bestScore.toFixed(2)}`;

    document.getElementById('fastestModel').innerText =
        fastestModel || '-';

    document.getElementById('fastestLatency').innerText =
        `${fastestLatency.toFixed(0)} ms`;

    document.getElementById('efficientModel').innerText =
        efficientModel || '-';

    document.getElementById('efficientScore').innerText =
        bestEfficiency.toFixed(4);

    document.getElementById('totalTokens').innerText =
        totalTokens.toLocaleString();
}

function renderCharts(results, validators) {

    const grouped = groupByModel(results);

    const models = Object.keys(grouped);

    const avgScores = models.map(m =>
        average(
            grouped[m]
                .map(x => Number(x.JudgeScore) || 0)
        )
    );

    const avgTokens = models.map(m =>
        average(
            grouped[m]
                .map(x => Number(x.TotalTokens) || 0)
        )
    );

    const avgLatency = models.map(m =>
        average(
            grouped[m]
                .map(x => Number(x.EndToEndLatencyMs) || 0)
        )
    );

    const avgEfficiency = models.map(m =>
        average(
            grouped[m]
                .map(x => Number(x.ScorePerToken) || 0)
        )
    );

    createBarChart(
        'scoreChart',
        models,
        avgScores,
        'Judge Score'
    );

    createBarChart(
        'tokensChart',
        models,
        avgTokens,
        'Tokens'
    );

    createBarChart(
        'latencyChart',
        models,
        avgLatency,
        'Latency (ms)'
    );

    createBarChart(
        'efficiencyChart',
        models,
        avgEfficiency,
        'Score / Token'
    );

    renderValidatorChart(validators);
}

function renderValidatorChart(validators) {

    const grouped = {};

    validators.forEach(v => {

        const validator =
            v.Validator || 'Unknown';

        if (!grouped[validator]) {

            grouped[validator] = {
                passed: 0,
                failed: 0
            };
        }

        grouped[validator].passed +=
            Number(v.Passed) || 0;

        grouped[validator].failed +=
            Number(v.Failed) || 0;
    });

    const labels =
        Object.keys(grouped);

    const passed =
        labels.map(x => grouped[x].passed);

    const failed =
        labels.map(x => grouped[x].failed);

    new Chart(
        document.getElementById('validatorChart'),
        {
            type: 'bar',

            data: {
                labels,
                datasets: [
                    {
                        label: 'Passed',
                        data: passed
                    },
                    {
                        label: 'Failed',
                        data: failed
                    }
                ]
            },

            options: {
                responsive: true,
                plugins: {
                    legend: {
                        labels: {
                            color: '#fff'
                        }
                    }
                },
                scales: {
                    x: {
                        ticks: {
                            color: '#fff'
                        }
                    },
                    y: {
                        ticks: {
                            color: '#fff'
                        }
                    }
                }
            }
        }
    );
}

function renderInsights(results) {

    const insights = [];

    const grouped = groupByModel(results);

    let bestEfficiencyModel = null;
    let bestEfficiency = -1;

    for (const model in grouped) {

        const efficiency =
            average(
                grouped[model]
                    .map(x => Number(x.ScorePerToken) || 0)
            );

        if (efficiency > bestEfficiency) {

            bestEfficiency = efficiency;
            bestEfficiencyModel = model;
        }
    }

    if (bestEfficiencyModel) {

        insights.push(
            `${bestEfficiencyModel} achieved the best score-per-token efficiency.`
        );
    }

    const models =
        Object.keys(grouped);

    if (models.length > 0) {

        const highestLatencyModel =
            models.sort((a, b) =>
                average(
                    grouped[b]
                        .map(x => Number(x.EndToEndLatencyMs) || 0)
                )
                -
                average(
                    grouped[a]
                        .map(x => Number(x.EndToEndLatencyMs) || 0)
                )
            )[0];

        insights.push(
            `${highestLatencyModel} presented the highest average latency.`
        );
    }

    const list =
        document.getElementById('insightsList');

    list.innerHTML = '';

    insights.forEach(i => {

        const div =
            document.createElement('div');

        div.className = 'insight-item';

        div.innerText = i;

        list.appendChild(div);
    });
}

function renderTable(results) {

    const tbody =
        document.querySelector('#resultsTable tbody');

    tbody.innerHTML = '';

    results.forEach(r => {

        const tr =
            document.createElement('tr');

        tr.innerHTML = `
            <td>${r.Model || '-'}</td>
            <td>${r.Action || '-'}</td>
            <td>${format(r.JudgeScore)}</td>
            <td>${format(r.TotalTokens)}</td>
            <td>${format(r.EndToEndLatencyMs)} ms</td>
            <td>${format(r.OutputEstimatedSmsSegmentsQtd)}</td>
            <td class="${r.Success ? 'success' : 'failure'}">
                ${r.Success ? 'PASS' : 'FAIL'}
            </td>
        `;

        tbody.appendChild(tr);
    });
}

function createBarChart(id, labels, data, label) {

    new Chart(
        document.getElementById(id),
        {
            type: 'bar',

            data: {
                labels,
                datasets: [
                    {
                        label,
                        data
                    }
                ]
            },

            options: {
                responsive: true,

                plugins: {
                    legend: {
                        labels: {
                            color: '#fff'
                        }
                    }
                },

                scales: {
                    x: {
                        ticks: {
                            color: '#fff'
                        }
                    },

                    y: {
                        ticks: {
                            color: '#fff'
                        }
                    }
                }
            }
        }
    );
}

function groupByModel(results) {

    const grouped = {};

    results.forEach(r => {

        const model =
            r.Model || 'Unknown';

        if (!grouped[model]) {

            grouped[model] = [];
        }

        grouped[model].push(r);
    });

    return grouped;
}

function average(arr) {

    if (!arr.length)
        return 0;

    return sum(arr) / arr.length;
}

function sum(arr) {

    return arr.reduce(
        (a, b) => a + b,
        0
    );
}

function format(v) {

    if (
        v === null ||
        v === undefined ||
        v === ''
    ) {
        return '-';
    }

    return Number(v).toFixed(2);
}

loadData();