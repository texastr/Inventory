var changedCnt: number = 0;//The number of elements that have been modified since the page loaded
function toggleApproval(elem: HTMLInputElement) {
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
    var btnSubmit: HTMLButtonElement = document.getElementById("btnSubmit") as HTMLButtonElement;
    var disabled = "disabled";//I HATE string literals
    if (changedCnt > 0) {
        btnSubmit.removeAttribute(disabled);
    }
    else {
        btnSubmit.setAttribute(disabled, disabled);
    }
}