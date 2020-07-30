function displayLoadingOverlay() {
    $('#loading-overlay').show();
}

function hideLoadingOverlay() {
    $('#loading-overlay').hide();
}

//近期密码功能将被登录验证取代
function verifyPassword() {
    var password = getCookie("adminPassword");
    if (password != "") {
    } else {
        password = prompt("请输入管理员密码：", "");
    }
    result = false;
    $.ajax({
        type: "GET",
        url: "/Home/VerifyPassword",
        data: { password: password },
        async: false,
        //contentType: "application/json;charset=utf-8"
    }).done(function (data) {
        if (data.message === 'Success') {
            setCookie("adminPassword", password, 7);
            result = true;
        }
        else
            alert('密码错误');
    }).fail(function (jqXHR, textStatus) {
        alert('密码错误');
    });
    return result;
}

function getCookie(cname) {
    var name = cname + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}
function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();

    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}