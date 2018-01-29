function CheckboxClicked() {
    var checkboxes = document.querySelectorAll('input[type="checkbox"]');
    for (var i in checkboxes) {
        if (checkboxes[i].checked) {
            document.getElementById("btnSubmit").disabled = false;
            return;
        }
    }
    document.getElementById("btnSubmit").disabled = true;
}
function IsDeleteConfirmed() {
    return confirm("Deleted files cannot be restored. Are you sure you want to delete the selected files?");
}
//# sourceMappingURL=CSV.js.map