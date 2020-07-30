
function recordCheckPageReady() {    
    recordGrid();
    $("#eventSelection").change(function (e) {        
        $('#recordGrid').GridUnload('#recordGrid');
        recordGrid();
    });
}

function recordGrid() {
    var model = getModel();
    if (model == null) {
        alert("获取群员与Boss资料失败");
        return;
    }

    $("#recordGrid").jqGrid({
        datatype: 'json',
        url: '/Record/GetJsonData',
        postData: { eventId: $("#eventSelection option:selected").val() },
        search: true,
        viewrecords: true,
        iconSet: "fontAwesome",
        sortname: 'recordTime',
        sortorder: 'desc',
        emptyrecords: 'No records',
        height: "auto",
        autowidth: true,
        rowNum: 30,
        rowList: [30, 90, 1000],
        cmTemplate: { labelClasses: "blue" },
        pager: "#recordPager",
        guiStyle: "bootstrap4",
        colNames: ['battleId', 'ID', 'Boss', '周目', '伤害', '类别', '出刀时间'],
        colModel: [
            { name: "battleId", key: true, hidden: true, search: false, editable: true },
            { name: "nickname", width: 150, editable: true, stype: 'select', searchoptions: { value: model.nicknames }, edittype: 'select', editoptions: { value: model.nicknames } },
            { name: "bossName", width: 120, editable: true, stype: 'select', searchoptions: { value: model.bossNames }, edittype: 'select', editoptions: { value: model.bossNames } },
            { name: "cycleNumber", width: 100, editable: false, align: 'center', searchoptions: { sopt: ['eq', 'ge', 'le'] }, searchrules: { integer: true } },
            { name: "damage", width: 100, editable: true, align: 'center', searchoptions: { sopt: ['eq', 'ge', 'le'] }, searchrules: { integer: true }, editrules: { number: true, minValue: 0 } },
            { name: "status", width: 100, editable: false, formatter: 'select', formatoptions: { value: 'Finished:整刀;Last_Hit:尾刀;OffTree:强行下树' }, stype: 'select', searchoptions: { value: ':All;Finished:整刀;Last_Hit:尾刀;OffTree:强行下树' } },
            {
                name: "recordTime", width: 200, editable: false, formatter: function (cellvalue, options, rowObject) {
                    if (cellvalue === null)
                        return '';
                    return new Date(cellvalue).toLocaleString(); // mm/dd/yyyy
                }}
        ]
    }).navGrid("#recordPager", {
        edit: true,
        add: false,
        del: false,
        search: false,
        refresh: true,
        closeAfterSearch: true
    }, {
            zIndex: 100,
            url: "/Home/EditRecord",
            datatype: 'json',
            mtype: 'Get',
            width: 500,
            height: 400,
            closeOnEscape: true,
            closeAfterEdit: true,
            beforeInitData: function (formid) {
                if (!verifyPassword())
                    return false;
            },
            afterComplete: function (response) {
                if (response.responseJSON.message != "Success")
                    alert(response.responseJSON.message)
            }
    }, {}, {}, { closeOnEscape: true });
    //$("#purchaseOrderGrid").jqGrid('filterToolbar');
    $("#recordGrid").jqGrid('filterToolbar', {
        stringResult: true,
        searchOnEnter: true,
        searchOperators: true,
        defaultSearch: 'eq',
        ignoreCase: true,
        operands: {
            "eq": "=", "ne": "!", "lt": "<", "le": "<=", "gt": ">", "ge": ">=", "cn": "~"
        },
        odata: [{ oper: 'eq', text: 'equal' }, { oper: 'ne', text: 'not equal' }, { oper: 'lt', text: 'less' },
        { oper: 'le', text: 'less or equal' }, { oper: 'gt', text: 'greater' }, { oper: 'ge', text: 'greater or equal' },
        { oper: 'cn', text: 'contains' }
        ]
    });
    $('[id ="gs_recordGrid_recordTime"]').attr('readonly', true);
    $('[id ="gs_recordGrid_recordTime"]').daterangepicker({
        singleDatePicker: true,
        showDropdowns: true,
        minDate: moment('2020-05-06'), maxDate: moment().endOf('year')
    });
    $('[id ="gs_recordGrid_recordTime"]').change(function () {
        $("#recordGrid")[0].triggerToolbar();
    });
}

function getModel() {
    var model = null;
    $.ajax({
        type: "GET",
        url: "/Home/GetModel",
        data: { eventId: $("#eventSelection option:selected").val() },
        async: false,
        //contentType: "application/json;charset=utf-8"
    }).done(function (data) {
        if (data.message === 'Success') {
            model = JSON.parse(data.model);
        }
        else
            alert(data.message);
    }).fail(function (jqXHR, textStatus) {
        alert(textStatus);
    });
    return model;
}