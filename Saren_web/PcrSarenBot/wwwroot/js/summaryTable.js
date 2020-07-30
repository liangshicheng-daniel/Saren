function summaryTablePageReady() {
    setTable();
    $("#eventSelection").change(function (e) {
        setTable();
    });
}

function setTable() {
    //var eventId = $("#eventSelection option:selected").val();
    
    $.ajax({
        type: "GET",
        url: "/Home/GetSummary",
        data: { eventId: $("#eventSelection option:selected").val() },
        async: false,
        //contentType: "application/json;charset=utf-8"
    }).done(function (data) {
        if (data.message === 'Success') {
            damageList = JSON.parse(data.damageList);
            nicknameList = JSON.parse(data.nicknameList);
            dateList = JSON.parse(data.dateList);
            drawTable(damageList, nicknameList, dateList);
        }
        else
            alert(data.message);
    }).fail(function (jqXHR, textStatus) {
        alert(textStatus);
    });
}

function drawTable(damageList, nicknameList, dateList) {
    //console.log(dateList);
    $("#summaryTableHead").empty();
    $("#summaryTableBody").empty();
    headerStr = "<tr><th></th>";
    var dayDamage = {};
    for (i = 0; i < dateList.length; i++) {
        headerStr += "<th>" + new Date(dateList[i]).toISOString().slice(5, 10) + "</th>";
        dayDamage[dateList[i]] = 0;
    }
    headerStr += "<th>每人输出</th></tr>";
    $("#summaryTableHead").append(headerStr);
    var memberDamage;  
    var overallDamage = 0;
    nicknameList.sort().forEach(function (nickname) {
        bodyStr = "<tr><td>" + nickname + "</td>";
        memberDamage = 0;
        for (i = 0; i < dateList.length; i++) {
            var damage = 0;
            if (damageList[nickname].find(x => x.date == dateList[i]) != null)
                damage = damageList[nickname].find(x => x.date == dateList[i]).damageSum;
            memberDamage += damage
            bodyStr += '<td>' + damage + '</td>'
            dayDamage[dateList[i]] += damage;
            overallDamage += damage;
        }
        bodyStr += "<td>" + memberDamage + "</td></tr>";
        console.log(bodyStr);
        $("#summaryTableBody").append(bodyStr);
    });
    bodyStr = "<tr><td>每日输出</td>";
    for (i = 0; i < dateList.length; i++) {
        bodyStr += '<td>' + dayDamage[dateList[i]] + '</td>'
    }
    bodyStr += "<td>" + overallDamage + "</td></tr>";
    $("#summaryTableBody").append(bodyStr);
}