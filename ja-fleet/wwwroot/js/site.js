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
                    {data: 'airlineNameJpShort'},
                    {data: 'typeName'},
                    {data: 'registrationNumber'},
                    {data: 'serialNumber'},
                    {data: 'registerDate'},
                    {data: 'operation'},
                    {data: 'wifi'},
                    {data: 'remarks'},
                ],
                paging:   false,
                ordering: false,
                fixedHeader: true,
                scrollY: '700px',
                scrollCollapse: true,
                columnDefs: [{
                    targets: '_all',
                    className: 'dt-center'
                }]

            });
}