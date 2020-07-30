var tabNames = ['#navBossOneTab', '#navBossTwoTab', '#navBossThreeTab', '#navBossFourTab', '#navBossFiveTab'];
var list;
function guidePageReady() {
    $("#newGuideDialog").dialog({
        autoOpen: false,
        height: 550,
        width: 520,
        modal: true,
        buttons: {
            "上传": newGuide,
            "取消": function () {
                $("#newGuideDialog").dialog("close");
            }
        },
        close: function () {
            //form[0].reset();
            $('#newGuideForm')[0].reset();
            $('#newGuideImageLabel').text('点击上传图片');
            $('#newGuideForm').removeClass('was-validated');
        }
    });

    $('.nav-boss-tabs').click(function (e) {
        e.preventDefault();
        if (!$(this).hasClass('guide_loaded')) {
            $(this).addClass('guide_loaded');
            loadGuide($(this).attr('bossId'), $(this).attr('href'));
        }
        $(this).tab('show');
    });
    tabNameSetup();
    $("#navBossOneTab").click();
    $("#eventSelection").change(function (e) {
        tabNameSetup();
        $('.nav-boss-tabs').removeClass('guide_loaded');
        $("#navBossOneTab").click();
    });

    $('#newGuide').click(function (e) {
        $("#newGuideDialog").dialog("open");
        $("#newGuideDialog").attr('guideId', 0);
    });

    $('#newGuideImage').change(function () {
        var fileName = $('#newGuideImage')[0].files[0].name;
        $('#newGuideImageLabel').text(fileName);
    });
    loadGuide(0, '#navBox');
    
}


function tabNameSetup() {
    $.ajax({
        type: "GET",
        url: "/Home/SetupTabs",
        data: { eventId: $("#eventSelection option:selected").val() },
        async: false,
        //contentType: "application/json;charset=utf-8"
    }).done(function (data) {
        if (data.message === 'Success') {
            list = JSON.parse(data.list);
            for (i = 0; i < list.length; i++) {
                $(tabNames[i]).attr('bossId', list[i].bossId);
                $(tabNames[i]).html('<b>' + list[i].bossName + '</b>')
            }

        }
        else
            alert(data.message);
    }).fail(function (jqXHR, textStatus) {
        alert(textStatus);
    });
}

function loadGuide(bossId, divId) {   
    displayLoadingOverlay();
    $.ajax({
        type: "POST",
        url: "/Home/GetGuides",
        data: {
            bossId: bossId
        }
        //contentType: "application/json;charset=utf-8"
    }).done(function (data) {
        if (data.message == 'Success') {
            $(divId).empty();
            list = JSON.parse(data.list);
            list.forEach(function (item) {
                writeToDiv(item, divId, bossId);
            });
        }
        else
            alert(data.message);
        hideLoadingOverlay();
    }).fail(function (jqXHR, textStatus) {
        alert('Error: ' + textStatus);
        hideLoadingOverlay();
    });
}

function newGuide() {
    if (!$('#newGuideForm')[0].checkValidity()) {
        $('#newGuideForm').addClass('was-validated');
        return;
    }
    
    displayLoadingOverlay();
    var totalFiles = document.getElementById("newGuideImage").files;

    var formData = new FormData();
    for (var i = 0; i < totalFiles.length; i++) {
        var file = totalFiles[i];
        formData.append("image", file);
    }
    formData.append('title', $("#newGuideTitle").val());
    formData.append('comment', $("#newGuideComment").val());

    var guideId = $("#newGuideDialog").attr('guideId');
    if (guideId == 0) {
        formData.append('eventId', $("#eventSelection option:selected").val());
        formData.append('bossId', $("a.active.nav-item").attr('bossId'));

        $.ajax({
            type: "POST",
            url: "/Home/CreateNewGuide",
            processData: false,
            contentType: false,
            data: formData
            //contentType: "application/json;charset=utf-8"
        }).done(function (data) {
            if (data.message == 'Success') {
                $("#newGuideDialog").dialog("close");
                $('#newGuideForm')[0].reset();
                loadGuide($("a.active.nav-item").attr('bossId'), $("a.active.nav-item").attr('href'));
            }
            else
                alert(data.message);
            hideLoadingOverlay();
        }).fail(function (jqXHR, textStatus) {
            alert('Error: ' + textStatus);
            hideLoadingOverlay();
        });
    }
    else {
        formData.append('guideId', guideId);
        $.ajax({
            type: "POST",
            url: "/Home/EditGuide",
            processData: false,
            contentType: false,
            data: formData
            //contentType: "application/json;charset=utf-8"
        }).done(function (data) {
            if (data.message == 'Success') {
                $("#newGuideDialog").dialog("close");
                $('#newGuideForm')[0].reset();
                loadGuide($("a.active.nav-item").attr('bossId'), $("a.active.nav-item").attr('href'));
            }
            else
                alert(data.message);
            hideLoadingOverlay();
        }).fail(function (jqXHR, textStatus) {
            alert('Error: ' + textStatus);
            hideLoadingOverlay();
        });
    }
}

function writeToDiv(item, divId, bossId) {
    var titleStr1 = '<div class="form-row"><div class="form-group col-md-2"></div><div class="form-group guide-content col-md-6"><h4><b id="guide_title_';
    var titleStr2 = '</b></h4></div><div class="form-group guide-content col-md-2"><button id="edit_guide_';
    var titleStr3 = '" type="button" class="btn btn-outline-primary btn-lg">编辑</button></div><div class="form-group guide-content col-md-2"><button id="delete_guide_';
    var titleStr4 = '" type="button" class="btn btn-outline-primary btn-lg">删除</button></div></div>';
    var contentStr1 = '<div class="form-row"><div class="form-group guide-button col-md-6">';
    var contentStr2 = '</div><div class="form-group guide-content col-md-6"><p id="guide_comment_';
    var contentStr3 = '</p></div></div>';
    var imageStr = "";
    if (item.imageUrl != null && item.imageUrl != "")
        imageStr = '<img class="guide-image" src="\\' + item.imageUrl + '">';
    
    $(divId).append('<hr/><br/>');
    $(divId).append(titleStr1 + item.guideId + '">' + item.title + titleStr2 + item.guideId + '" guideId="' + item.guideId + titleStr3 + item.guideId + titleStr4);
    if (item.comment != null)
        item.comment = item.comment.replace(/\n/g, "<br/>");
    else
        item.comment = '';
    $(divId).append(contentStr1 + imageStr + contentStr2 + item.guideId + '">' + item.comment + contentStr3);

    $('#delete_guide_' + item.guideId).click(function (e) {
        if (!verifyPassword())
            return false;
        $.ajax({
            type: "POST",
            url: "/Home/RemoveGuides",
            data: {
                guideId: item.guideId
            }
            //contentType: "application/json;charset=utf-8"
        }).done(function (data) {
            if (data.message == 'Success') {
                loadGuide(bossId, divId);
            }
            else
                alert(data.message);
        }).fail(function (jqXHR, textStatus) {
            alert('Error: ' + textStatus);
        });
    });

    $('#edit_guide_' + item.guideId).click(function (e) {
        if (!verifyPassword())
            return false;
        var guideId = $(this).attr('guideId');
        //console.log(guideId);
        $("#newGuideTitle").val($('#guide_title_' + guideId).html());
        $("#newGuideComment").val($('#guide_comment_' + guideId).html().replace(/\<br\/\>/g, "\n").replace(/\<br\>/g, "\n"));
        $("#newGuideDialog").dialog("open");
        $("#newGuideDialog").attr('guideId', guideId);
    });
}