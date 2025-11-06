
// Manage view: priority selector behaviour
document.addEventListener('DOMContentLoaded', () => {
    const hiddenPriorityInput = document.getElementById('HiddenPriority');
    if (!hiddenPriorityInput) return;

    const priorityButtons = Array.from(document.querySelectorAll('.priority-btn'));
    if (!priorityButtons.length) return;

    const setActivePriority = (value) => {
        priorityButtons.forEach((btn) => {
            const isActive = btn.dataset.priorityName === value;
            btn.classList.toggle('active', isActive);
        });
        if (value) {
            hiddenPriorityInput.value = value;
        }
    };

    const initialPriority = hiddenPriorityInput.dataset.initialPriority || hiddenPriorityInput.value;
    if (initialPriority) {
        setActivePriority(initialPriority);
    }

    priorityButtons.forEach((button) => {
        button.addEventListener('click', () => {
            setActivePriority(button.dataset.priorityName);
        });
    });
});

