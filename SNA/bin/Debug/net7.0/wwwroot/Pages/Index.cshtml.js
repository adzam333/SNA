(function () {

    $("#browseButton").click(function (e) {
        e.preventDefault();
        $("#datasetInputPath").click();
    });

    $("#datasetInputPath").change(function (e) {
        e.preventDefault();
        var file = $(this)[0].files[0];
        var filePath = $(this).val();
        var datasetName = $("#datasetName").val();
        $("#datasetInputName").val(file.name)
    });

    $("#datasetUploadBtn").click(function (e) {
        e.preventDefault();
        var fileInput = $("#datasetInputPath")[0];
        var file = fileInput.files[0];
        var datasetName = $("#datasetName").val();
        var fileFormData = new FormData();
        fileFormData.append('file', file);

        var alert = $("#uploadAlert");
        alert.html("Uploading dataset. Please wait...");
        alert.show();
        sNA.services.dataset.populateDataset(file, datasetName,
            {
                data: fileFormData,
                processData: false,
                contentType: false
            }).then(function (result) {
                var button = $('<button>')
                    .addClass('datasetItem list-group-item list-group-item-action')
                    .attr('onclick', "openModal('" + result.name + "')")
                    .text(result.name);

                $('#DatasetList').append(button);


                alert.html('Dataset uploaded.');
                alert.show()
                setTimeout(function () {
                    alert.hide();
                }, 3000);
            }).catch(function (error) {
                alert.html('Dataset upload failed. Error: ' + error.message);
                alert.show();
                setTimeout(function () {
                    alert.hide();
                }, 3000);
            });

    });
})();

function openModal(datasetName) {
    var dataset = sNA.services.dataset.getDataset(datasetName)
        .then(function (result) {
            var modal = $('#modalBody');
            // modal.find('.modal-header h5').text(datasetName);
            var content = ` 
            <table class="table table-bordered">
                <thead class="thead-dark">
                    <tr>
                        <th scope="col">Name of Dataset</th>
                        <th scope="col">Average connections</th>
                        <th scope="col">Number of users</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>${result.name}</td>
                        <td>${result.averageConnections}</td>
                        <td>${result.userCount}</td>
                    </tr>
                </tbody>
            </table>
        `;
            modal.html(content);
            $('#myModal').modal('show');


        });

}
