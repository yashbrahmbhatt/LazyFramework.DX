<script lang="ts" module>
	import type { Snippet } from 'svelte';

	export interface FadeInOutProps {
		delayIn: number;
		delayOut: number;
		outDirection: 'up' | 'down' | 'left' | 'right';
		inDirection: 'up' | 'down' | 'left' | 'right';
		duration?: number;
		class?: string;
		children?: Snippet;
	}
</script>

<script lang="ts">
	let {
		delayIn = 500,
		delayOut = 3000,
		children,
		inDirection = 'down',
		outDirection = 'up',
		duration = 500,
		class: className = ''
	}: FadeInOutProps = $props();
	let textDisappear = $state(false);
	let textVisible = $state(false);
	let element: HTMLDivElement;
	// Mapping directions to translate classes
	const translateClasses = {
		up: 'translate-y-[-100%]',
		down: 'translate-y-[100%]',
		left: 'translate-x-[-100%]',
		right: 'translate-x-[100%]',
		none: ''
	};
	// Fade in after the specified delay
	if (delayIn > 0) {
		transitionIn(delayIn);
	}
	function transitionIn(delay: number) {
		setTimeout(() => {
			textVisible = true;
		}, delay);
	}

	function transitionOut(delay: number, dur: number) {
		setTimeout(() => {
			textDisappear = true;
		}, delay);
		setTimeout(() => {
			element.remove();
		}, delay + dur);
	}
	// Fade out after the specified delay
	if (delayOut > 0) {
		transitionOut(delayOut, duration);
	}
</script>

<div
	bind:this={element}
	class="{className} transition-all duration-500 duration-[{duration}ms] ease-in-out
    {textDisappear
		? `${translateClasses[outDirection]} opacity-0`
		: textVisible
			? 'translate-x-0 translate-y-0 opacity-100'
			: `${translateClasses[inDirection]} opacity-0`}"
>
	{@render children?.()}
	<!-- Content inside the component -->
</div>
