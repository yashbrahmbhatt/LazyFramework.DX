<script lang="ts" module>
	export type EditableToggleCellProps = EditableBase & {
		trueText?: string;
		falseText?: string;
	};
</script>

<script lang="ts">
	import Switch from '$lib/components/ui/switch/switch.svelte';
	import Label from '$lib/components/ui/label/label.svelte';
	import type { EditableBase } from './editable-base';

	let {
		trueText,
		falseText,
		context,
		setValue,
		store,
		getValue,
		validations
	}: EditableToggleCellProps = $props();
	let value = $state(getValue($store, context));
	let errors = $state(validate());

	store.subscribe(() => {
		value = getValue($store, context);
		errors = validate();
	});
	function validate() {
		return validations.map((validation) => validation(value, $store, context));
	}

	function update() {
		if (errors.every((error) => error === null)) {
			setValue(store, value, context);
		}
	}
</script>

<div class="flex w-full flex-col gap-1.5">
	<div class="flex items-center space-x-2">
		<Switch bind:checked={value} onCheckedChange={update} />
		<Label>{value ? (trueText ?? value) : (falseText ?? value)}</Label>
	</div>
	{#each errors as error}
		{#if error}
			<p class="text-wrap text-xs text-destructive">{error}</p>
		{/if}
	{/each}
</div>
