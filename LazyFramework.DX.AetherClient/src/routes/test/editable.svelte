<script lang="ts">
	import Input from '$lib/components/ui/input/input.svelte';

	let { value = $bindable(), updated } = $props();
</script>

<Input bind:value class="" oninput={() => updated(value)} />
