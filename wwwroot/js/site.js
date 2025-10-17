
//manage priority button selection and hidden input update
document.addEventListener('DOMContentLoaded', function () {
    const hiddenPriorityInput = document.getElementById('HiddenPriority');
    const priorityButtons = document.querySelectorAll('.priority-btn');

    // Initialize the hidden field with the current value
    // Note: If you use the Enum value (int), you need to convert it here. 
    // The Razor code below sets the name (string) by default for the hidden input.
    // Let's ensure the default active button is set based on the current model value.
    if (hiddenPriorityInput && '@Model.Ticket.Priority' !== '') {
        // If the model priority is set, ensure the hidden field reflects the string name
        hiddenPriorityInput.value = '@Model.Ticket.Priority';
    }

    // Add click listener to all buttons
    priorityButtons.forEach(button => {
        button.addEventListener('click', function() {
            // Remove 'active' class from all buttons
            priorityButtons.forEach(btn => btn.classList.remove('active'));

            // Add 'active' class to the clicked button
            this.classList.add('active');

            // Update the hidden input field with the priority name (e.g., "P1")
            hiddenPriorityInput.value = this.dataset.priorityName;
        });
    });

    // Set the initial active state based on the current model value
    // This ensures the button is highlighted on page load
    if ('@Model.Ticket.Priority' !== '') {
        const currentPriority = '@Model.Ticket.Priority';
        const activeBtn = document.querySelector(`.priority-btn[data-priority-name="${currentPriority}"]`);
        if (activeBtn) {
            activeBtn.classList.add('active');
        }
    }
});

