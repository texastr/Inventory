function CheckboxClicked() {
    let checkboxes = <NodeListOf<HTMLInputElement>> <any> document.querySelectorAll('input[type="checkbox"]');
    for (let i in checkboxes) {
        if (checkboxes[i].checked) {
            (<HTMLButtonElement>document.getElementById("btnSubmit")).disabled = false;
            return;
        }
    }
    (<HTMLButtonElement>document.getElementById("btnSubmit")).disabled = true;
}

function IsDeleteConfirmed() {
    return confirm("Deleted files cannot be restored. Are you sure you want to delete the selected files?")
}