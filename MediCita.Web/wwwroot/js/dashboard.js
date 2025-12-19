document.addEventListener("DOMContentLoaded", () => {
    loadDashboard();
});

async function loadDashboard() {
    try {
        const res = await fetch('/Admin/ObtenerEstadisticas');
        if (!res.ok) throw new Error();

        const data = await res.json();

        animateNumber("totalPacientes", data.totalPacientes ?? 0);
        animateNumber("totalMedicos", data.totalMedicosRegistrados ?? 0);
        animateNumber("citasHoy", data.citasHoy ?? 0);
        document.getElementById("ventasMes").textContent =
            "S/ " + (parseFloat(data.ventasMesActual || 0).toFixed(2));

        renderTopMedicamentos(data.topMedicamentosJson);
        renderStockBajo(data.productosStockBajo, data.medicamentosStockBajoJson);

    } catch {
        document.querySelectorAll(".number")
            .forEach(n => n.textContent = "Error");
    }
}

function animateNumber(id, value) {
    const el = document.getElementById(id);
    let current = 0;
    const step = Math.max(1, value / 40);

    const interval = setInterval(() => {
        current += step;
        if (current >= value) {
            el.textContent = value;
            clearInterval(interval);
        } else {
            el.textContent = Math.floor(current);
        }
    }, 20);
}

function renderTopMedicamentos(json) {
    const list = document.getElementById("topMedicamentos");
    list.innerHTML = "";

    let items = [];
    try { items = JSON.parse(json || "[]"); } catch { }

    if (!items.length) {
        list.innerHTML = `<li class="list-group-item text-center text-muted">
            No hay datos
        </li>`;
        return;
    }

    items.forEach((m, i) => {
        list.innerHTML += `
            <li class="list-group-item d-flex justify-content-between">
                <span><strong>#${i + 1}</strong> ${m.Nombre}</span>
                <span class="badge bg-success">${m.TotalVendido} und.</span>
            </li>
        `;
    });
}

function renderStockBajo(count, json) {
    if (!count || count <= 0) return;

    document.getElementById("cantidadStockBajo").textContent = count;
    document.getElementById("alertaStockBajo").classList.remove("d-none");

    const ul = document.getElementById("listaStockBajo");
    ul.innerHTML = "";

    let productos = [];
    try { productos = JSON.parse(json || "[]"); } catch { }

    productos.forEach(p => {
        ul.innerHTML += `<li>${p.Nombre} (Stock: ${p.Stock})</li>`;
    });
}
