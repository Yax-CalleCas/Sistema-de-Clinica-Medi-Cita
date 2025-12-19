$(document).ready(function () {

    // cargar médicos por especialidad
    $("#cbEspecialidad").change(function () {
        let id = $(this).val();
        $("#cbMedico").empty();

        fetch(`/Citas/ListarMedicos?idEspecialidad=${id}`)
            .then(r => r.json())
            .then(data => {
                $("#cbMedico").append(`<option value="">-- Seleccione --</option>`);
                data.forEach(m => {
                    $("#cbMedico").append(`<option value="${m.idMedico}">${m.nombreCompleto}</option>`);
                });
            });
    });

    // obtener detalles del médico
    $("#cbMedico").change(function () {
        let id = $(this).val();
        if (!id) return;

        fetch(`/Citas/ObtenerDetallesMedico?idMedico=${id}`)
            .then(r => r.json())
            .then(m => {
                if (!m) return;

                $("#div-info-medico").removeClass("d-none");
                $("#medico-detalles").html(`
                    <p><strong>Precio:</strong> S/ ${m.precioConsulta}</p>
                    <p><strong>Duración:</strong> ${m.duracionMinutos} min</p>
                    <p><strong>Horario:</strong> ${m.horarioAtencionInicio} - ${m.horarioAtencionFin}</p>
                `);
            });
    });

    // obtener horarios disponibles
    $("#btnBuscarHorarios").click(function () {
        let medico = $("#cbMedico").val();
        let fecha = $("#txtFecha").val();

        if (!medico || !fecha) {
            alert("Seleccione médico y fecha");
            return;
        }

        fetch(`/Citas/ObtenerHorarios?idMedico=${medico}&fecha=${fecha}`)
            .then(r => r.json())
            .then(data => {
                $("#div-horarios").load("/Citas/_PartialHorarios", function () {
                    $("#div-horarios").html(data.length === 0
                        ? "<div class='alert alert-warning'>No hay horarios disponibles.</div>"
                        : generarListaHorarios(data)
                    );
                });
            });
    });

    // crear cita
    $("#btnCrearCita").click(function () {
        let idPaciente = 1; // aquí pones el user logueado
        let idMedico = $("#cbMedico").val();
        let fecha = $("#txtFecha").val();
        let hora = $(".horario.active").data("hora");

        let model = {
            idPaciente,
            idMedico,
            fecha,
            hora
        };

        fetch('/Citas/CrearCita', {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(model)
        })
            .then(r => r.json())
            .then(resp => {
                alert(resp.mensaje);
                if (resp.ok) location.reload();
            });
    });

});

function generarListaHorarios(data) {
    let html = `<ul class="list-group">`;
    data.forEach(h => {
        html += `
            <li class="list-group-item horario" data-hora="${h.horaDisponible}">
                ${h.horaDisponible} - S/ ${h.montoConsulta}
            </li>`;
    });
    html += `</ul>`;

    return html;
}
