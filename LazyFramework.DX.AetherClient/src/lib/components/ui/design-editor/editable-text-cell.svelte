<script lang="ts" module>
	export type EditableTextCellProps = EditableBase & {};
</script>

<script lang="ts">
	import type { EditableBase } from './editable-base';
	import Input from '../input/input.svelte';

	let { context, store, getValue, validations, setValue }: EditableTextCellProps = $props();
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
	<Input bind:value class="" oninput={update} />

	<!-- Display error message if validation fails -->
	{#each errors as error}
		{#if error}
			<p class="text-wrap text-xs text-destructive">{error}</p>
		{/if}
	{/each}
</div>
