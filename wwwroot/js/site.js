
var jaosIsUpdating = false;
const defaultCulture = "es-AR";
const defaultLanguage = "es";
const cultureDisposableNumberChars = /[.$]/g; // chars to be ignored when user inputs a number
const cultureDecimalSeparator = ',';


//================ go to previous page ==================================
$(document).on('click', '#goBack', function () {
    history.go(-1);
});



// (document).ready()
//================ Select2: select with search ==================================
//================ Month/Year dates ==================================

$(document).ready(function () {
    // Prepare searchable Text fields
    if ($.fn.select2) {
        $(".searchableSelect").select2({
            language: defaultLanguage
        });
    }

    // Configure datepicker for Month/Year fields
    $('.monthDate').datepicker({
        format: 'mm/yyyy',
        startView: 'months',
        minViewMode: 'months',
        autoclose: true,
        language: defaultLanguage
    });

});


//================ format a number ==================================
function formatNumber(object, decimals) {
    try {
        const value = parseFloat(object.value.trim()
            .replace(cultureDisposableNumberChars, '')
            .replace(cultureDecimalSeparator, '.'));
        if (!isNaN(value))
            object.value = value.toLocaleString(defaultCulture, decimals >= 0 ?
                { minimumFractionDigits: decimals, maximumFractionDigits: decimals, } : {});
    }
    catch { }
}

function numberValue(object) {
    const value = parseFloat(object.val().trim()
        .replace(cultureDisposableNumberChars, '')
        .replace(cultureDecimalSeparator, '.'));
    if (!isNaN(value))
        return (value);
    try {
    }
    catch { }
    return 0;
}


//================ Select for many to many relationships ==================================

// move selected option from source (dropdownlist) Select to Select with id=destintationId (boxlist)
// sourceStartPosition: used to indicate if the Select has a general title that should not be moved (ex. 'Select an option') 
// 0: use all positions   1: skip first item
function addSelectedOption(source, destinationId, sourceStartPosition) {
    if (source.selectedIndex >= sourceStartPosition) {
        var destination = document.getElementById(destinationId);
        event.stopImmediatePropagation();
        destination.add(
            source.item(source.selectedIndex),
            findPosInSelect(source.item(source.selectedIndex), destination, 0));
        event.stopImmediatePropagation();
    }
    selectAll(destination);
}

// remove from box and include again in select with destinationId
// destinationStartPosition: used to indicate if the Select has a general title that should not be moved (ex. 'Select an option') 
// 0: use all positions   1: skip first item
function removeSelectedOption(source, destinationId, destinationStartPosition) {
    var destination = document.getElementById(destinationId);
    destination.add(
        source.item(source.selectedIndex),
        findPosInSelect(source.item(source.selectedIndex), destination, destinationStartPosition)
    );
    selectAll(source);
    if (destinationStartPosition == 0) {
        destination.item(0).selected = true;
    }
    else {
        deselectAll(destination);
    }
}

// finds the position where an item should be inserted to keep the items sorted in the select.
function findPosInSelect(elem, select, startPosition) {
    var pos = startPosition;
    while ((pos < select.length) && (select.item(pos).text.toLowerCase() < elem.text.toLowerCase())) {
        pos++;
    }
    return pos;
}

function selectAll(select) {
    if (select != null)
        if (select.length > 0)
            for (i = 0; i < select.length; i++)
                select.item(i).selected = true;
}

// to deselect all and select them on submit (change both moveSelectedOption to deselect)
function deselectAll(select) {
    if (select != null)
        if (select.length > 0)
            for (i = 0; i < select.length; i++)
                select.item(i).selected = false;
}
//function selectFields(selectId) {
//    var select = document.getElementById(selectId);
//    selectAll(select);
//}
//$('#projectForm').submit(function () {
//    selectFields('selectedLocation_project');
//    return true;
//})

//---------------------- end Select for many to many relationships ---------------------------------


function isFirefox() {
    return (navigator.userAgent.toLowerCase().indexOf('firefox') > -1);
}

// ============================
// Table header sorting carets. 
// Sets the carets for the sorting titles in a table.
$(document).ready(function () {
    $('.sort-header').on('click', function () {
        if ($(this).find('i').hasClass('bi-caret-down')) {
            $(this).find('i').removeClass('bi-caret-down').addClass('bi-caret-down-fill');
        } else if ($(this).find('i').hasClass('bi-caret-down-fill')) {
            $(this).find('i').removeClass('bi-caret-down-fill').addClass('bi-caret-up-fill');
        } else if ($(this).find('i').hasClass('bi-caret-up-fill')) {
            $(this).find('i').removeClass('bi-caret-up-fill').addClass('bi-caret-down-fill');
        }
    });
});


