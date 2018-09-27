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
                scrollX: true,
                scrollCollapse: true,
                columnDefs: [{
                    targets: '_all',
                    className: 'dt-center'
                }]

            });
}