//If you're calling shit on this page, you might need to also include <script src="~/js/3rdParty/Load-Image/js/load-image.all.min.js"></script>
//in the calling page
///<reference path="../../node_modules/@types/jquery/index.d.ts"/>
;
//I think I need to do this to make the validator bullshit work
/*interface JQuery<TElement,HTMLElement> {
    validator: any;
}*/
function addFileField(outermostDivID, templateField) {
    var template = document.getElementById(templateField);
    var newField = template.cloneNode(true);
    newField.removeAttribute("id");
    document.getElementById(outermostDivID).appendChild(newField);
}
function initializeFields(parentField, templateField, fieldsCnt) {
    var i;
    for (i = 0; i < fieldsCnt; i++) {
        addFileField(parentField, templateField);
    }
}
function previewFile(inputElement) {
    var file = inputElement.files[0];
    var options = { orientation: true, maxWidth: 200, maxHeight: 200 };
    var loadingImage = loadImage(file, function (img) {
        var div = inputElement.parentNode.parentNode.getElementsByClassName("imagePreview")[0];
        while (div.firstChild) {
            div.removeChild(div.firstChild);
        }
        div.appendChild(img);
    }, options);
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
    //jk. the actual last thing is to trigger validation
    //(JQuery as any).validator.unobtrusive.parse(document);
    if ($('#fileInputer').valid()) {
    }
}
//Functions to handle details/delete views
function displayImageFromURL(URL, divName) {
    var options = { orientation: true, maxWidth: 500, maxHeight: 500 };
    var callbacker = function (e) {
        document.getElementById(divName).appendChild(e);
    };
    var img = loadImage(URL, callbacker, options);
}
//this makes the client-side validation work to count that there are five files. Some mix of MS, Michael, SO, and moi
jQuery(document).ready(function ($) {
    jQuery.validator.addMethod('sufficientImages', function (value, element, params) {
        var elems = document.getElementsByClassName("newImages");
        var cnt = elems.length;
        var minFiles = +params[0]; //I thought i could use params.cnt. But if params[0] works, I won't complain
        var filesCnt = 0;
        for (var i = 0; i < cnt; i++) {
            var elem = elems.item(i);
            if (elem.value !== null && elem.value.length > 0) {
                filesCnt++;
            }
        }
        elems = document.getElementsByClassName("oldImages");
        cnt = elems.length;
        for (var j = 0; j < cnt; j++) {
            var elem = elems.item(j);
            if (!elem.checked) {
                filesCnt++;
            }
        }
        return filesCnt >= minFiles;
    });
    jQuery.validator.unobtrusive.adapters.add('sufficientImages', ['cnt'], //No idea what this parameter does. But shit breaks if it's messed with
    function (options) {
        options.rules['sufficientImages'] = [options.params.cnt];
        options.messages['sufficientImages'] = options.message;
    });
});
//region: trash
function helloWorld() { alert("hello"); }
function OldaddFileField(outermostDivID) {
    var wrapperDiv = document.createElement("div");
    wrapperDiv.classList.add("col-mod-10");
    var span = document.createElement("span");
    //This inspired by the HTML that is generated by asp-for=ImageFile
    var field = document.createElement("input");
    field.type = "file";
    field.name = "ImageFilePathsnewnewnew";
    field.id = "ImageFiles";
    field.accept = "image/*";
    field.classList.add("input-validation-error");
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
    previewDiv.style.cssText = "clear: both";
    wrapperDiv.appendChild(span);
    wrapperDiv.appendChild(document.createElement("br"));
    wrapperDiv.appendChild(previewDiv);
    document.getElementById(outermostDivID).appendChild(wrapperDiv);
}
//# sourceMappingURL=product.js.map