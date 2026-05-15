window.datepicker = {
    init: function (element, options, dotNetHelper) {
        options = options || {};
        
        // Default configurations
        const config = {
            allowInput: true,
            dateFormat: "d-m-Y",
            altInput: true,
            altFormat: "F j, Y",
            ...options,
            onChange: function (selectedDates, dateStr, instance) {
                dotNetHelper.invokeMethodAsync('OnDateChanged', dateStr);
            }
        };

        if (typeof flatpickr === 'undefined') {
            console.error('Flatpickr is not loaded. Please check CDN connection.');
            return;
        }
        const fp = flatpickr(element, config);
        element._flatpickr = fp;
    },
    setValue: function (element, value) {
        if (element && element._flatpickr) {
            element._flatpickr.setDate(value, false);
        }
    },
    destroy: function (element) {
        if (element && element._flatpickr) {
            element._flatpickr.destroy();
        }
    }
};
