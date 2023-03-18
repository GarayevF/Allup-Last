$(document).ready(function () {

    toastr.options = {
        "closeButton": false,
        "debug": false,
        "newestOnTop": false,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }

    let successInput = $("input[name='success']");
    if (successInput.val()?.length > 0) {
        toastr["success"](successInput.val())
    }

    let errorInput = $("input[name='error']");
    if (errorInput.val().length > 0) {
        toastr["error"](errorInput.val())
    }

    $(document).on('click', '.addAddresses', function () {
        e.preventDefault();

        $('.addressForm').removeClass('d-none');
        $('.addressesContainer').addClass('d-none');
    })

    $(document).on('click', '.product-close, .basketdelete, .sub', function (e)
    {
        e.preventDefault();


        let url = $(this).attr('href');
        console.log(url)
        fetch(url)
            .then(res => {
                return res.text()
            })
            .then(data => {
                $('.header-cart').html(data)
                console.log(url.replace("removebasket/" + url.split('/')[url.split('/').length - 1], 'mainbasket'))
                fetch(url.replace("removebasket/" + url.split('/')[url.split('/').length - 1], 'mainbasket'))
                    .then(res2 => {
                        return res2.text()
                    })
                    .then(data2 => {
                        $('.cart-main-checkoutpage').html(data2)
                    })
            })
    }).on('click', '.add', function (e) {
        e.preventDefault();

        let url = $(this).attr('href');
        console.log(url)
        fetch(url)
            .then(res => {
                return res.text()
            })
            .then(data => {
                $('.cart-main-checkoutpage').html(data)
            })
    })

    $('.addbasket').click(function (e) {
        e.preventDefault();

        let url = $(this).attr('href');
        console.log(url)
        fetch(url)
            .then(res => {
                
                return res.text()
            })
            .then(data => {
                $('.header-cart').html(data)
            })
    })

    $('.search').keyup(function () {
        let search = $(this).val();
        let categoryId = $('.category').val();

        if (search.length >= 3) {
            fetch('/product/search?search=' + search + '&categoryId=' + categoryId)
                .then(res => {
                    return res.json()
                })
                .then(data => {
                    $('.searchBody').html(data)
                })
        } else {
            $('.searchBody').html('')
        }
    })

    $('.productModal').click(function (e) {
        e.preventDefault();

        let url = $(this).attr('href');

        fetch(url)
            .then(res => {
                return res.text()
            })
            .then(data => {
                $('.modal-content').html(data)
                $('.quick-view-image').slick({
                    slidesToShow: 1,
                    slidesToScroll: 1,
                    arrows: false,
                    dots: false,
                    fade: true,
                    asNavFor: '.quick-view-thumb',
                    speed: 400,
                });

                $('.quick-view-thumb').slick({
                    slidesToShow: 4,
                    slidesToScroll: 1,
                    asNavFor: '.quick-view-image',
                    dots: false,
                    arrows: false,
                    focusOnSelect: true,
                    speed: 400,
                });
            })
    })
})