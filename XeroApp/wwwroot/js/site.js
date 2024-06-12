// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

$(document).ready(function () {

  


    var ids = []

    $("#add").click(() => {
        let id = $("#clientid").val()
        let secret = $("#clientsecret").val()

        let clientId = {
            id: id,
            secret: secret
        }

        ids.push(clientId)
        localStorage.setItem("ids", JSON.stringify(ids))
    })

    $("#refresh").click(() => loadClientIds())

    function loadClientIds() {
        let idss = localStorage.getItem("ids")
        if (idss != null || idss != undefined) {
            ids = JSON.parse(idss)

            ids.forEach((value) => {
                console.log(value)
                $("#ids").val(value)
            })
        }
    }



    var currentTemplate = "fbox-blue";
    $(".fbox-blue").css({
        "opacity": "0",
        "display": "block",
    }).show().animate({ opacity: 1 }, 1500)


    setInterval(function () {

        var template = ['fbox-blue', 'fbox-lightgray', 'fbox-rainbow'];
      
        var randomItem = template[Math.floor(Math.random() * template.length)];

        if (currentTemplate != randomItem) {


            $("." + currentTemplate).fadeOut("slow");


            template.forEach(function (string, i) {

                if (currentTemplate != template[i]) {
                    $("." + template[i]).hide();
                }
            });

            $("." + randomItem).css({
                "opacity": "0",
                "display": "block",
            }).fadeIn().animate({ opacity: 1 }, 1500)
            currentTemplate = randomItem;
        }

    }, 10000);






});

