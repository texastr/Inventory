//If you're calling shit on this page, you might need to also include <script src="~/js/3rdParty/Load-Image/js/load-image.all.min.js"></script>
//in the calling page
function addFileField(outermostDivID) {
    var wrapperDiv = document.createElement("div");
    wrapperDiv.classList.add("col-md-10");

    var span = document.createElement("span");
    
    var field = document.createElement("input");
    field.type = "file";
    field.name = "upload";
    field.accept = "image/*";
    field.setAttribute("onchange", "previewFile(this);"); //This was the only way to set the onchange property
    
    var xButton = document.createElement("button");
    xButton.type = "button";
    xButton.innerHTML = "🗙";
    xButton.style.visibility = "hidden";
    xButton.setAttribute("onclick", "resetField(this);");

    span.appendChild(field);
    span.appendChild(xButton);

    var previewDiv = document.createElement("div"); //The image canvas will go in here.
    previewDiv.className = "imagePreview";

    //I don't understand why how or why this floating stuff works. but it works now, so that's great.
    field.style.cssFloat = "left";
    previewDiv.style.cssFloat = "left";
    wrapperDiv.appendChild(span);
    wrapperDiv.appendChild(document.createElement("br"));
    wrapperDiv.appendChild(previewDiv);
    document.getElementById(outermostDivID).appendChild(wrapperDiv);
}
 
function initializeFields(elemID,fieldsCnt) {
    for (i = 0; i < fieldsCnt; i++) {
        addFileField(elemID);
    }
}
function previewFile(inputElement) {
    var file = inputElement.files[0];
    var options = { orientation: true, maxWidth: 200, maxHeight: 200 };
    var loadingImage = loadImage(file,
        function (img) {
            var div = inputElement.parentNode.parentNode.getElementsByClassName("imagePreview")[0];
            while (div.firstChild) {
                div.removeChild(div.firstChild);
            }
            div.appendChild(img);
        },
        options);
    inputElement.parentNode.getElementsByTagName("button")[0].style.visibility = "visible";
}

function resetField(element) {
    var span = element.parentNode;
    var field = span.getElementsByTagName("input")[0];
    field.value = "";

    //Now let's get rid of the image
    var div = span.parentNode;
    var previewDiv = div.getElementsByClassName("imagePreview")[0];
    previewDiv.innerHTML = "";

    //lastly, let's hide the button.
    element.style.visibility = "hidden";


}

//Functions to handle details/delete views
function displayImageFromURL(URL,divName){
    var options = { orientation: true, maxWidth: 500, maxHeight: 500 };

    var callbacker = function (e) {
        document.getElementById(divName).appendChild(e);
    };

    var img = loadImage(URL, callbacker, options);
}

function helloWorld() { alert("hello");}
//var imgURL = "https://texastr.blob.core.windows.net/images/2017-11-02-20-09-20-562$20161208_094535.jpg";
