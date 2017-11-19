$(function () {
    //$("#boxid").attr("disabled", "disabled");
    //$("#quantity").attr("disabled", "disabled");
    $("#locationid").change(function () {
        var str = $("#locationid option:selected").text();
        var b = parseInt(str.substring(8, 11), 0);
        $('#capacity').empty();
        for (var i = 0; i <= (100 - b) ; i++) {
            $('#capacity')
             .append($("<option></option>")
             .attr("value", i)
             .text(i + "%"));
        }
        $("#capacity").attr("disabled", false);

    });

    //$("#qty").change(function () {

    //});
    $("#qty").on('change keyup paste', function () {
        if (Math.floor($("#qty").val()) == $("#qty").val() && $.isNumeric($("#qty").val())) {
            $("#boxid").attr("disabled", false);
            $("#quantity").attr("disabled", false);
        } else {
            $("#boxid").attr("disabled", "disabled");
            $("#quantity").attr("disabled", "disabled");
        }
    });

    $("#quantity").change(function () {
        AddARow();
    });

    $("#capacity").change(function () {
        AddARow();
    });

})

function AddARow() {
    if (($("#locationid").val() != "") && ($("#capacity").val() != null) && ($("#qty").val() != "") && ($("#qunatity").val() != "")) {
        //alert("GOOD");
        $('tr:has(td:contains("' + $("#locationid").val() + '"))').remove();//Remove existing inventory if any for proper order
        var uuid = guid();
        var oldCapacity = parseInt($("#locationid option:selected").text().substring(8, 11), 0);
        var newCapacity = oldCapacity + parseInt($("#capacity").val());
        //$("#invHidden").append("<input type='hidden' name='hdnInventory' value='" + $("#inventoryid").val() + ":" + $("#qty").val() + "'/>");
        $("#invTable").append("<tr id='" + uuid + "'><td>" + $("#locationid").val() + " : " + newCapacity + "%</td><td>" + $("#lot").val() + "</td><td>" + $("#boxqty").val() +
                              "</td><td>" + $("#itembox option:selected").text() + "</td><td>" + $("#qty").val() +
                              "</td><td><span class='delete' onclick=\"javascript:DeleteItem('" + uuid + "');\">DELETE</span>" +
                              "<input type='hidden' name='hdnInventory' value='" + $("#locationid").val() + ":" + $("#lot").val() + ":" + $("#boxqty").val() +
                              ":" + $("#itembox").val() + ":" + $("#qty").val() + ":" + newCapacity + "'/></td></tr>");
        $("#divList").show();

        //Reset
        $("#locationid option[value='" + $("#locationid").val() + "']").text($("#locationid").val()+" : " + newCapacity + "% filled");
        $("#locationid").val('');
        $("#qty").val('0');
        $("#boxqty").val('');
        $("#itembox").val('');
        $("#capacity").empty();
        $("#capacity").attr("disabled", "disabled");
        $(".field-validation-error").text('');
        $("#lot").val('');
    }
}
function DeleteItem(id) {
    $("#" + id).remove();
    if ($("input[name=hdnInventory]").length == 0) {
        $("#divList").hide();
    }
}

var guid = (function () {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
                   .toString(16)
                   .substring(1);
    }
    return function () {
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
               s4() + '-' + s4() + s4() + s4();
    };
})();

function CallPrint(strid) {
    var prtContent = document.getElementById(strid);
    var WinPrint = window.open('', '', 'letf=0,top=0,width=600,height=800,toolbar=0,scrollbars=0,status=0');
    WinPrint.document.write(prtContent.innerHTML);
    WinPrint.document.close();
    WinPrint.focus();
    WinPrint.print();
    WinPrint.close();
}