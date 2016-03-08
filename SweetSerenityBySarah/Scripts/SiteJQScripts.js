(function ($) {

    //set the functions included
    var siteScripts = {
        onReady: function () {
            this.formDateClose();
            this.videoModalClose();
            this.gallerySlider();
            this.googleAnalytics();
        },

        //define what each function does
        formDateClose: function () {
            $(".date input").datepicker({
                format: "dd/mm/yyyy",
                startDate: "-3d"
            });
        },

        //define what each function does
        videoModalClose: function () {
            $("#videoModal").on("hide.bs.modal", function (e) {
                $("video#pageVideo").get(0).pause();
            });
            $("#videoModal").on("show.bs.modal", function (e) {
                $("video#pageVideo").get(0).play();
            });
        },

        //define what each function does
        gallerySlider: function () {
            $('#borderless-checkbox').on('change', function () {
                var borderless = $(this).is(':checked');
                $('#blueimp-gallery').data('useBootstrapModal', !borderless);
                $('#blueimp-gallery').toggleClass('blueimp-gallery-controls', borderless);
            });

            $('#fullscreen-checkbox').on('change', function () {
                $('#blueimp-gallery').data('fullScreen', $(this).is(':checked'));
            });

            $('#image-gallery-button').on('click', function (event) {
                event.preventDefault();
                blueimp.Gallery($('#links a'), $('#blueimp-gallery').data());
            });
        },

        //define what each function does
        googleAnalytics: function() {
            (function (i, s, o, g, r, a, m) {
                i['GoogleAnalyticsObject'] = r; i[r] = i[r] || function () {
                    (i[r].q = i[r].q || []).push(arguments)
                }, i[r].l = 1 * new Date(); a = s.createElement(o),
                m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m)
            })(window, document, 'script', '//www.google-analytics.com/analytics.js', 'ga');

            ga('create', 'UA-64349053-1', 'auto');
            ga('send', 'pageview');
        }

    };

    // on doc ready load the defined main function
    $(document).ready(function () {
        siteScripts.onReady();
    });

})(jQuery);