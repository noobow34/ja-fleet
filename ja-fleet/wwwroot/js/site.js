// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function setupTable(tableName,url)
{
            $.extend( $.fn.dataTable.defaults, {
                language: {
                    url: "https://cdn.datatables.net/plug-ins/9dcbecd42ad/i18n/Japanese.json"
                }
            });
            $(tableName).DataTable({
                ajax: {url:url,
                        dataSrc: ''},
                columns: [
                    {data: 'airline'},
                    {data: 'type'},
                    {data: 'registrationNumber'},
                    {data: 'serialNumber'},
                    {data: 'registrationDate'},
                    {data: 'status'},
                    {data: 'wifi'},
                    {data: 'remarks'},
                ],
                "paging":   false,
                "ordering": false,
                "columnDefs": [{
                    "targets": '_all',
                    "className": 'dt-center'
                }]

            });
}