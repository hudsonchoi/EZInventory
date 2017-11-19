$(function () {
    if (location.pathname.indexOf("Company") >= 0) {
        $("#company").addClass("hilite");
        $("#company a").css("color", "#ffffff");
        $("#company a").css("font-weight", "bold");
    } else if (location.pathname.indexOf("Item") >= 0) {
        $("#item").addClass("hilite");
        $("#item a").css("color", "#ffffff");
        $("#item a").css("font-weight", "bold");
    } else if (location.pathname.indexOf("Inventory") >= 0) {
        $("#inventory").addClass("hilite");
        $("#inventory a").css("color", "#ffffff");
        $("#inventory a").css("font-weight", "bold");
    } else if (location.pathname.indexOf("Order") >= 0) {
        $("#order").addClass("hilite");
        $("#order a").css("color", "#ffffff");
        $("#order a").css("font-weight", "bold");
    } else if (location.pathname.indexOf("Location") >= 0) {
        $("#location").addClass("hilite");
        $("#location a").css("color", "#ffffff");
        $("#location a").css("font-weight", "bold");
    } else {
        $("#home").addClass("hilite");
        $("#home a").css("color", "#ffffff");
        $("#home a").css("font-weight", "bold");
    }
});