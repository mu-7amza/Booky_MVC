var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#myTable').DataTable({
        "ajax": { url: '/admin/company/getall' },
        "columns": [
            { data: 'name', "width": "20%" },
            { data: 'streatAddress', "width": "15%" },
            { data: 'city', "width": "15%" },
            { data: 'state', "width": "10%" },
            { data: 'postalCode', "width": "15%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75" btn-group role="group">
                            <a class="btn btn-primary mx-2" href="/admin/company/upsert?id=${data}"><i class="bi bi-pencil-square"> Edit</i></a>
                            <a class="btn btn-danger mx-2" onClick=Delete('/admin/company/delete/${data}')><i class="bi bi-pencil-square"> Delete</i></a>
                            </div>`
                },
                "width": "25%"
            }

        ]
    });
}

function Delete(url) {

    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload()
                    toastr.success(data.message);
                }
            })
        }
    })
}