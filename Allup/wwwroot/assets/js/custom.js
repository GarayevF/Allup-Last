$(document).ready(function () {

    $(document).on('click', '.product-close', function (e) {
        e.preventDefault();

        let url = $(this).attr('href');
        console.log(url)
        fetch(url)
            .then(res => {
                return res.text()
            })
            .then(data => {
                $('.header-cart').html(data)
                fetch(url.replace("removebasket/" + url.split('/')[url.split('/').length - 1], 'mainbasket'))
                
                    .then(res2 => {
                        return res2.text()
                    })
                    .then(data2 => {
                        $('.cart-main-checkoutpage').html(data2)
                    })
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