<script lang="ts">
	let { delayIn = 500, delayOut = 3000, children, duration = 500, class: className = "" } = $props();
	let textDisappear = $state(false);
	let textVisible = $state(false);
    let element: HTMLDivElement;
	// Fade in after the specified delay
	setTimeout(() => {
		textVisible = true;
	}, delayIn);

	// Fade out after the specified delay
	setTimeout(() => {
		textDisappear = true;

	}, delayOut);
    setTimeout(() => {
        element.remove();
    }, delayOut + duration);
</script>
<div bind:this={element}
	class="{className} transition-all duration-500 ease-in-out
    {textDisappear
		? 'translate-y-[-200%] opacity-0'
		: textVisible
			? 'translate-y-0 opacity-100'
			: 'translate-y-[100%] opacity-0'}"
>
	{@render children()}
	<!-- Content inside the component -->
</div>
