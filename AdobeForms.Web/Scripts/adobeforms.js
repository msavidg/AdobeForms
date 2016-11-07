
//This function serializes all inputs within the bootstrap main container 'Content' class.
$.fn.serializeObject = function () {

    var formData = {};
    var formArray = this.serializeArray();

    for (var i = 0, n = formArray.length; i < n; ++i) {
        if ($('#' + formArray[i].name).prop('type') !== 'undefined' && ($('#' + formArray[i].name).prop('type') === 'checkbox')) {
            //Convert the value to boolean. The raw value shows up as 'on', which translates to false in the viewmodel(if your property in the ViewModel is boolean)
            formArray[i].value = $('#' + formArray[i].name).is(":checked");
        }

        if ($('#' + formArray[i].name).prop('type') !== 'undefined' && $('#' + formArray[i].name).prop('type') === 'radio') {
            $("input:radio[name='\' +  formArray[i].name +  '\']:checked").val();
        }

        formData[formArray[i].name] = formArray[i].value;

    }

    //Serialize all inputs that are disabled and send the values. This is because Html does not post disabled inputs causing this inputs to have null values.
    var fields = $("#content :input").not("button"); //exclude buttons

    fields.each(function (index, element) {

        var isDisabled = $(element).is(':disabled');

        if (isDisabled) {
            if ($(element).prop('type') !== 'undefined' && ($(element).prop('type') === 'checkbox')) {
                //Convert the value to boolean. The raw value shows up as 'on', which translates to false in the viewmodel(if your property in the ViewModel is boolean)
                $(element).val($(element).is(":checked"));
            }

            if ($(element).prop('type') !== 'undefined' && ($(element).prop('type') === 'radio')) {
                //Convert the value to boolean. The raw value shows up as 'on', which translates to false in the viewmodel(if your property in the ViewModel is boolean)
                $(element).val($(element).is(":checked"));
            }

            formData[$(element).prop('id')] = $(element).val();
            //$(element).prop('disabled', false);
        }
    });

    return formData;
};
