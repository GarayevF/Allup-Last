$(document).ready(function () {

    let isMainVal = $('#IsMain').val();

    let path = window.location.pathname.split('/');
    let action = path[3];
    let controller = path[2];

    if (action.toLowerCase() == 'create' && controller.toLowerCase() == 'category') {
        let isMain = $("#IsMain").is(':checked');

        if (isMain) {
            $('.fileInput').removeClass('d-none');
            $('.parentInput').addClass('d-none');
        } else {
            $('.fileInput').addClass('d-none');
            $('.parentInput').removeClass('d-none');
        }
    }

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