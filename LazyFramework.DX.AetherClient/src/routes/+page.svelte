<script lang="ts">
	import { goto } from '$app/navigation';
	import Button from '$lib/components/ui/button/button.svelte';
	import FadeInOut from '$lib/composites/FadeInOut.svelte';
	let { data } = $props();

	let solutionJsonExists = $derived(
		Object.keys(data.Files).some((key) => key.endsWith('solution.project.json'))
	);
	$inspect(solutionJsonExists);

	let messages = $derived(
		solutionJsonExists
			? ['Welcome back', "Looks like you're ready to go", 'Enjoy :)']
			: ['Welcome', 'Looks like this is your first time here', "Let's get started :)"]
	);

	$inspect(data);
	let con: boolean = $state(false);
	setTimeout(() => {
		con = true;
	}, 1000);
</script>

<div class="flex h-[95vh] w-full flex-col place-content-center space-y-4">
	<div class="flex -translate-y-12 flex-col gap-4">
		{#each messages as message, i}
			<FadeInOut
				class="self-center"
				delayIn={500 + i * 2500}
				delayOut={i !== 2 ? 2500 + i * 2500 : -1}
				inDirection="down"
				outDirection="up"
			>
				<h1 class="text-center text-4xl font-bold">{message}</h1>
			</FadeInOut>
		{/each}
		{#if !solutionJsonExists}
			<FadeInOut
				class=""
				delayIn={messages.length * 2500 - 1000}
				delayOut={-1}
				inDirection="down"
				outDirection="up"
			>
				<div class="flex w-full flex-row justify-center gap-4">
					<Button onclick={() => goto('/setup')}>Setup Wizard</Button>
				</div>
			</FadeInOut>
		{:else}
			<FadeInOut
				inDirection="down"
				outDirection="up"
				class=""
				delayIn={messages.length * 2500 - 1000}
				delayOut={-1}
			>
				<div class="flex w-full flex-row justify-center gap-4">
					<Button onclick={() => goto('/solution')}>Solution Explorer</Button>
				</div>
			</FadeInOut>
		{/if}
	</div>
</div>

<style>
</style>
