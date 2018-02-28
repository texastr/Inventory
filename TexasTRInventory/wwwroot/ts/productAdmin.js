var changedCnt = 0; //The number of elements that have been modified since the page loaded
function toggleApproval(elem) {
    if (elem.style.border == "") {
        elem.style.border = "thin solid #ff0000";
        changedCnt++;
        //alert(changedCnt);
    }
    else {
        elem.style.border = "";
        changedCnt--;
        //alert(changedCnt);
    }
    var btnSubmit = document.getElementById("btnSubmit");
    var disabled = "disabled"; //I HATE string literals
    if (changedCnt > 0) {
        btnSubmit.removeAttribute(disabled);
    }
    else {
        btnSubmit.setAttribute(disabled, disabled);
    }
}
//# sourceMappingURL=productAdmin.js.map