$(function () {
    var masonryCall;

    $(window).on('load', function () {
        var allPosts = $('.feeds-post-item');

        if (allPosts.hasClass('facebook-item')) {
            addClickEvent("facebook");
        }
        if (allPosts.hasClass('twitter-item')) {
            addClickEvent("twitter");
        }
        if (allPosts.hasClass('instagram-item')) {
            addClickEvent("instagram");
        }
        if (allPosts.hasClass('googleplus-item')) {
            addClickEvent("googleplus");
        }
        if (allPosts.hasClass('pinterest-item')) {
            addClickEvent("pinterest");
        }

        $(".feeds-nav .all-tab").click(function () {
            $('.feeds-nav-item').removeClass('active');
            $(this).addClass('active');

            allPosts.show().addClass("shown");

            refreshMasonryGrid();
        });

        $('.feeds-select').on('change', function () {
            if ($('.feeds-select').attr('value') == "All") {
                allPosts.show().addClass("shown");
                refreshMasonryGrid();
            }
            else{
                handleClick($('.feeds-select').attr('value'));
            }
        });

        var rtlValue = $('.social-feed').attr("data-rtl") === 'True' || $('.feeds-page').attr("data-rtl") === 'True';
        masonryCall = $('.feeds-post-list').masonry({
            itemSelector: '.feeds-post-item',
            columnWidth: '.feeds-post-item-sizer',
            percentPosition: true,
            originLeft: !rtlValue
        });

        $('.feed-loader-wrapper').hide();
    });

    function addClickEvent(socialNetwork) {
        $(".feeds-nav ." + socialNetwork + "-tab").click(function () {
            handleClick(socialNetwork);
        });
    }

    function handleClick(socialNetwork) {
        $('.feeds-nav-item').removeClass('active');
        $('.feeds-nav-item.' + socialNetwork + '-tab').addClass('active');

        $(".feeds-post-item").not('.' + socialNetwork + '-item')
            .removeClass("shown")
                .delay(200)
                    .queue(function (next) {
                        $(this).hide();
                        next();
                    });

        $(".feeds-post-item." + socialNetwork + "-item").show().addClass("shown");

        refreshMasonryGrid();
    }

    function refreshMasonryGrid() {
        setTimeout(function () {
            masonryCall.masonry('layout');
        }, 200);
    }
});