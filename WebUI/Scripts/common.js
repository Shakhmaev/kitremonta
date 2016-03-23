/// <reference path="jquery-2.2.0.js" />

$(function () {

});

/*function equal() {
    var currentTallest = 0,
     currentRowStart = 0,
     rowDivs = new Array(),
     $el,
     topPosition = 0;

    $('.block').each(function() {

        $el = $(this);
        topPosition = $el.position().top;
   
        if (currentRowStart != topPosition) {

            // we just came to a new row.  Set all the heights on the completed row
            for (currentDiv = 0 ; currentDiv < rowDivs.length ; currentDiv++) {
                rowDivs[currentDiv].height(currentTallest);
            }

            // set the variables for the new row
            rowDivs.length = 0; // empty the array
            currentRowStart = topPosition;
            currentTallest = $el.height();
            rowDivs.push($el);

        } else {

            // another div on the current row.  Add it to the list and check if it's taller
            rowDivs.push($el);
            currentTallest = (currentTallest < $el.height()) ? ($el.height()) : (currentTallest);

        }
   
        // do the last row
        for (currentDiv = 0 ; currentDiv < rowDivs.length ; currentDiv++) {
            rowDivs[currentDiv].height(currentTallest);
        }
    })
}*/

function initDropDownChange(conf) {

    var conf = conf;

    $("select#SubCategory_ParentID").change(function () {
        LoadSubsDropDown();
    });

    function LoadSubsDropDown() {
        $.ajax({
            url: conf.loadsubs,
            data: { val: $("select#SubCategory_ParentID").val() } ,
            success: function (list) {
                $("select#SubCategory_CategoryId").html("");
                $.each(JSON.parse(list), function (k, v) {
                    var arr = [];
                    count = 0;
                    $.each(v, function (i, g) {
                        arr[count] = g;
                        count++;
                    })
                    $("select#SubCategory_CategoryId").append("<option value=" + arr[0] + ">" + arr[1] + "</option>");
                });

                $("select#SubCategory_CategoryId option[value='" + conf.currentcategory + "']").attr("selected", true);
            }
        })
    }

    LoadSubsDropDown();
}

jQuery(document).ready(function () {
    /*if ($("#PopupWrapper").length) {
        var dialog = jQuery("#PopupWrapper").dialog({
            autoOpen: false,
            title: 'Добавлено',
            resizeable: false,
            show: "clip",
            hide: "slide",
            width: 'auto',
            fluid: true,
            buttons: [
                {
                    text: "Продолжить покупки",
                    class: "btn btn-success btn-continue",
                    click: function () {
                        jQuery(this).dialog("close");
                        picToCartAnimation();
                    }

                },
                {
                text: "Оформить заказ",
                class: "btn btn-success btn-cart",
                click: function () {
                    window.location.href = $(".btn-tocart").attr("href");
                }
        }
            ],
            modal: true,
            dialogClass: "success-dialog",
            open: function (e, ui) {
                $(this).parent().find(".ui-dialog-buttonpane button")
                    .addClass("orange");
            }
        })
    };*/

    AddedItemPopUp = function (img) {
        //dialog.dialog("open");
        $("#PopupWrapper").html("<img src='" + img + "'/>");
        ModalButtonsRemove();
        $("#myModal").modal("show");
    };

    ModalButtonsRemove = function() {
        $(".modal-footer").hide();
    }
    ModalButtonsAdd = function() {
        $(".modal-footer").show();
    }

    $('#myModal').on('hidden.bs.modal', function (e) {
        if ($(".modal-title").text()!="Не было добавлено в корзину")
        picToCartAnimation();
    });
});



function SetPicId(id, url, obj) {
    $("#picid").remove();
    if ($(".well[id='" + id + "']").length > 0) {
        $(".well[id='" + id + "']").append("<div id='picid' style='position:absolute;'></div>");
    }
    else if ($(".row[id='" + id + "']").length > 0) {
        $(".row[id='" + id + "']").append("<div id='picid' style='position:absolute;'></div>");
    }
    $("#picid").css({ display:"none"});
    $("#picid").html("<img src='"+url+"' style='max-width:100px;max-height:100px;'/>");
}

function picToCartAnimation() {
    $("#picid").css({display:"block"});
    $("#picid").animate({ left: $("li.dropdown").offset().left - $("#picid").parent().offset().left, top: $("li.dropdown").offset().top - $("#picid").parent().offset().top }, 5000)
    .animate({ opacity: "hide" }, 1000);
}

function js(args) {
    var args = args;
    ReloadExtraImages = function() {
        var ur = args.url1;
        $.ajax({
            url: args.url1,
            data: { id: args.id },
            success: function (data) {
                $("#links").empty();
                $.each(JSON.parse(data), function (i, s) {
                    $("#links").append(
                        '<div class="image">' +
                            '<a href="' + s + '" title=' + args.name + ' data-gallery>' +
                                '<img src="' + s + '" class="image" max-width=100 height=100 alt='+args.name+'>' +
                            '</a>' +
                            '<input type="button" id="' + s.replace('/ImageView/GetImageById/', '') + '" class="btn btn-primary btn-del-img" value="X" />' +
                        '</div>');

                })
            }
        });
    }

    ReloadExtraImagesNoX = function () {
        $.ajax({
            url: args.url1,
            success: function (data) {
                console.log(data);
                var x = data.length;
                console.log(JSON.parse(data));
                $("#img_00").parent().wrap('<div class="col-xs-6 col-lg-3 image">');
                $("#img_00").addClass("image");
                $.each(JSON.parse(data), function (i, s) {
                    $("#gallery_01").append(
                        '<div class="col-xs-6 col-lg-3 image">' +
                            '<a href="#" data-image="' + s + '" data-zoom-image="' + s + '">' +
                                '<img id="img_0'+(i+1)+'" src="' + s + '">' +
                            '</a>' +
                        '</div>'
                            );

                })
                initzoom();
            }
        });
    }

    gotoCart = function () {
        window.location.href = args.urlCart;
    }

    js.gotoCart = gotoCart;
    js.ReloadExtraImages = ReloadExtraImages;
    js.ReloadExtraImagesNoX = ReloadExtraImagesNoX;

    
    //Скрипт для загрузки и обновления основной фотографии-->
    $('#upload').click(function () {
            var formdata = new FormData();
            var fileInput = document.getElementById('ImgMainFile');
            for (i = 0; i < fileInput.files.length; i++) {
                formdata.append(fileInput.files[i].name, fileInput.files[i]);
            }
            if (formdata!=null){
                formdata.append("id", args.id);
                var xhr = new XMLHttpRequest();
                xhr.open('POST', args.UrlUploadMainPhoto);
                xhr.send(formdata);
                xhr.onreadystatechange = function () {
                    if (xhr.readyState == 4 && xhr.status == 200) {
                        $("#mainimg").attr("src",args.GetImagesUrlWithStamp+(new Date().getTime()));
                        fileInput.value='';
                        $("#upload-file-info").html('Файл не выбран');
                    }
                }
            }
            return false;
        });

    //Скрипт для загрузки и обновления дополнительных фотографий-->
        $('#uploadExtra').click(function () {
            var formdata = new FormData();
            var fileInput = document.getElementById('ImgExtraFiles');
            for (i = 0; i < fileInput.files.length; i++) {
                formdata.append(fileInput.files[i].name, fileInput.files[i]);
            }
            if (formdata!=null){
                formdata.append("id", args.id);
                var xhr = new XMLHttpRequest();
                xhr.open('POST', args.UrlUploadExtraPhoto);
                xhr.send(formdata);
                xhr.onreadystatechange = function () {
                    if (xhr.readyState == 4 && xhr.status == 200) {

                        js.ReloadExtraImages();

                        fileInput.value='';
                        $("#upload-file-info2").html('Файл не выбран');
                    }
                }
            }
            return false;
        });

    //Удаление доп.фото
    $(document).on('click','.btn-del-img',(function () {
        var value = $(this).attr("id");
        //alert("123");
        $.ajax({
            url: args.UrlDeletePhoto,
            data: { Id:args.id, PhotoId:value },
            success: function(){ 
                //alert("123");
                ReloadExtraImages();}
        });
    }));
}

window.onscroll = function vverh() {
    var up = document.getElementById('up');
    if (up != null) {
        up.style.display = (window.pageYOffset > '1000' ? 'block' : 'none')
    };
}

function initzoom() {

    if ($(window).width() > 656) {
        var config = {
            gallery: 'gallery_01',
            cursor: 'crosshair',
            galleryActiveClass: 'active',
            imageCrossfade: true,
            loadingIcon: false,
            zoomWindowWidth: 300,
            zoomWindowHeight: 300,
            zoomWindowFadeIn: 500, 
            zoomWindowFadeOut: 500,
            constrainType: 'width',
            easing: true,
            responsive:true
        };
    }
    else
    {
        var config = {
            gallery: 'gallery_01',
            galleryActiveClass: 'active',
            imageCrossfade: true,
            loadingIcon: false,
            zoomType: 'inner',
            zoomWindowWidth: 200,
            zoomWindowHeight: 200,
            constrainType: 'width',
            responsive: true
        };
    }
    var image = $('#gallery_01 a');
    var zoomImage = $('img#zoom_03');

    zoomImage.elevateZoom(config);

    image.on('click', function () {
        // Remove old instance od EZ
        $('.zoomContainer').remove();
        zoomImage.removeData('elevateZoom');

        zoomImage.hide();
        // Update source for images
        zoomImage.data('zoom-image', $(this).data('zoom-image'));

        zoomImage.attr('src', $(this).data('image'));
        // Reinitialize EZ
        zoomImage.elevateZoom(config);
    });
}

//slider

function initslider(conf) {

    var slider = document.getElementById('range');

    var slider = noUiSlider.create(slider, {
        start: [parseInt(conf.lower, 10), parseInt(conf.higher, 10)], // Handle start position
        connect: true, // Display a colored bar between the handles
        behaviour: 'tap-drag', // Move handle on tap, bar is draggable
        range: { // Slider can select '0' to '100'
            'min': [ 0 ],
            'max': parseInt(conf.higher,10)
        }
    });

    var valueElements = [
        document.getElementById('lowerCurrent'),
        document.getElementById('higherCurrent')
    ];

    slider.on('update', function (values, handle) {
        valueElements[handle].innerHTML = values[handle];
        $("#lowerCurrent").data("lower", values[0]);
        $("#higherCurrent").data("higher", values[1]);
    });
}

$(document).ready(function () {

    $(document).on('click', ".linkbtn", function (event) {
        event.preventDefault(); //проверка на нажатие уже включенной страницы
        var conf = initsum(this.id);
        loadsummary(conf);
        loadsummary.begin();
    });

        $(document).on('click', ".next", function (event) {
            event.preventDefault();
            if (!$(".next").is(":disabled")){
                var conf = initsum(-2);
                loadsummary(conf);
                loadsummary.begin();
            }
        });

        $(document).on('click', ".prev", function (event) {
            event.preventDefault();
            if (!$(".prev").is(":disabled")){
                var conf = initsum(-3);
                loadsummary(conf);
                loadsummary.begin();
            }
        });

    $(document).on('click', ".filter", function (event) {
        event.preventDefault();
        var conf = initsum();
        loadsummary(conf);
        loadsummary.begin();
    });

    $(document).on("click", ".dropdown-menu *", function (e) {
        e.stopImmediatePropagation();
    });

    
    if ($('#mySpinbox').length > 0) {
        $('.spinbox#mySpinbox').each(function (index) {
            $(this).spinbox({
                'min':1
                
            });
        });
    }

});

function CartIndexInit(updateurl, forwardurl) {
    $("#Order").click(function (e) {
        e.preventDefault();
        var list = [];
        $("tbody > tr").each(function(){
            var id = $(this).attr("id");
            var spinbox = $(this).find("#mySpinbox");
            if (spinbox.length <= 0) {
                spinbox = $(this).find("#spin_" + id + "");
                var packs = $(this).find('.packs');
                if (packs.length > 0) count = packs.find(".badge").text();
                else count = spinbox.spinbox('value');
            }
            else var count = $(this).find("#mySpinbox").spinbox('value');
            list.push({id:id, quantity:count});
        });

        var DTO = { cartid:null, list: list };

        $.ajax({
            contentType: "application/json; charset=utf-8",
            type:'POST',
            url: updateurl,
            data: JSON.stringify(DTO),
            success: function () {
                location.href = forwardurl;
            }
        });
    });
}

function loadsummary (conf) {
    begin = function (id) {
        $.ajax({
            url: conf.url,
            type: 'POST',
            data: {
                category:conf.category,
                filters: {
                    HigherPrice: parseInt(conf.high),
                    LowerPrice: parseInt(conf.low),
                    PageSize: parseInt(conf.ps),
                    Hot: conf.hot,
                    WithDiscount: conf.discount,
                    SortBy: conf.sort,
                    SelectedBrands: conf.brands,
                    SelectedCountries: conf.countries,
                    SelectedPurposes: conf.purposes
                },
                page: conf.page
            },
            success: function (data) {
                $(".load").html(data);
                $("html,body").finish();
                $("html,body").animate({
                    scrollTop: $(".load").offset().top },
                    "slow");
            },
            error: function (response) {
            //alert(response);  
        }
        });
    }
    loadsummary.begin = begin;
};

