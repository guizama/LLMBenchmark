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

    const metricsRaw =
        await fetch('model_metrics.json')
            .then(r => r.json());

    const benchmarkResults =
        extractArray(benchmarkResultsRaw);

    const validators =
        extractArray(validatorsRaw);

    const runs =
        extractArray(runsRaw);

    const metrics =
        extractArray(metricsRaw);

    initializeDashboard(
        benchmarkResults,
        validators,
        runs,
        metrics
    );
}

function extractArray(obj) {

    if (Array.isArray(obj))
        return obj;

    const firstKey =
        Object.keys(obj)[0];

    return obj[firstKey] || [];
}

function initializeDashboard(results, validators, runs, metrics) {

    const latestRun = runs[0];

    if (latestRun) {

        document.getElementById('runStatus').innerText =
            `${latestRun.Status} · ${latestRun.TotalExecutions} execuções`;
    }

    renderCards(results, metrics);

    renderCharts(results, validators, metrics);

    renderInsights(results, metrics);

    renderTable(results);
}

function renderCards(results, metrics) {

    const grouped = groupByModel(results);

    let bestModel = null;
    let bestScore = -1;

    let fastestModel = null;
    let fastestLatency = Number.MAX_VALUE;

    let efficientModel = null;
    let bestEfficiency = -1;

    for (const model in grouped) {

        const items = grouped[model];

        const avgScore =
            average(items.map(x => Number(x.JudgeScore) || 0));

        const avgLatency =
            average(items.map(x => Number(x.EndToEndLatencyMs) || 0));

        const avgEfficiency =
            average(items.map(x => Number(x.ScorePerToken) || 0));

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
        `Score ${safeToFixed(bestScore, 2)}`;

    document.getElementById('fastestModel').innerText =
        fastestModel || '-';

    document.getElementById('fastestLatency').innerText =
        `${safeToFixed(fastestLatency, 0)} ms`;

    document.getElementById('efficientModel').innerText =
        efficientModel || '-';

    document.getElementById('efficientScore').innerText =
        safeToFixed(bestEfficiency, 4);

    const benchmarkTokens =
        sum(metrics.map(x => Number(x.benchmark_total_tokens) || 0));

    const judgeTokens =
        sum(metrics.map(x => Number(x.judge_total_tokens) || 0));

    document.getElementById('benchmarkTokens').innerText =
        benchmarkTokens.toLocaleString();

    document.getElementById('judgeTokens').innerText =
        judgeTokens.toLocaleString();
}

function renderCharts(results, validators, metrics) {

    const grouped = groupByModel(results);

    const models = Object.keys(grouped);

    const avgScores = models.map(m =>
        average(
            grouped[m]
                .map(x => Number(x.JudgeScore) || 0)
        )
    );
	
	const avgBenchmarkTokens = models.map(m => {
		const metric =
			metrics.find(x => x.Model === m);

		return Number(metric?.benchmark_avg_tokens) || 0;
	});

	const avgJudgeTokens = models.map(m => {

		const metric =
			metrics.find(x => x.Model === m);

		return Number(metric?.judge_avg_tokens) || 0;
	});

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
        'Score Médio'
    );

    createDualBarChart( 
		'tokensChart', 
		models, 
		avgBenchmarkTokens, 
		avgJudgeTokens, 
		'Benchmark Tokens', 
		'Judge Tokens' 
	);

    createBarChart(
        'latencyChart',
        models,
        avgLatency,
        'Latência'
    );

    createBarChart(
        'efficiencyChart',
        models,
        avgEfficiency,
        'Score por Token'
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

    const passRates =
        labels.map(x => {

            const total =
                grouped[x].passed + grouped[x].failed;

            if (!total)
                return 0;

            return (
                grouped[x].passed / total
            ) * 100;
        });

    new Chart(
        document.getElementById('validatorChart'),
        {
            type: 'bar',

            data: {
                labels,
                datasets: [
                    {
                        label: 'Taxa de Aprovação (%)',
                        data: passRates
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
                        },
                        max: 100
                    }
                }
            }
        }
    );
}

function renderInsights(results, metrics) {

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
            `${bestEfficiencyModel} apresentou a melhor eficiência score/token.`
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
            `${highestLatencyModel} apresentou a maior latência média.`
        );
    }

    if (metrics.length > 0) {

        const highestJudgeTokens =
            metrics.sort(
                (a, b) =>
                    Number(b.judge_total_tokens || 0)
                    -
                    Number(a.judge_total_tokens || 0)
            )[0];

        insights.push(
            `${highestJudgeTokens.Model} consumiu mais tokens no processo de validação Judge.`
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
                ${r.Success ? 'PASSOU' : 'FALHOU'}
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

function safeToFixed(value, decimals = 2) {

    if (
        value === null ||
        value === undefined ||
        isNaN(value)
    ) {
        return '0';
    }

    return Number(value).toFixed(decimals);
}

function createDualBarChart(
    id,
    labels,
    data1,
    data2,
    label1,
    label2
) {

    new Chart(
        document.getElementById(id),
        {
            type: 'bar',

            data: {
                labels,
                datasets: [
                    {
                        label: label1,
                        data: data1
                    },
                    {
                        label: label2,
                        data: data2
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



loadData();