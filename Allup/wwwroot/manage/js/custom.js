$(document).ready(function () {
    $('#IsMain').click(function () {
        let isMain = $(this).is(':checked');

        if (isMain) {
            $('.fileInput').removeClass('d-none');
            $('.parentInput').addClass('d-none');
        } else {
            $('.fileInput').addClass('d-none');
            $('.parentInput').removeClass('d-none');
        }
    })
})