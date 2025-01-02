<script lang="ts" module>
	export type EditableTextAreaCellProps = EditableBase & {};
</script>

<script lang="ts">
	import type { EditableBase } from './editable-base';
	import { Textarea } from '../textarea';

	let { context, store, getValue, validations, setValue }: EditableTextAreaCellProps = $props();
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

<div class="flex max-w-sm flex-col gap-1.5">
	<Textarea bind:value class="" oninput={update} />

	<!-- Display error message if validation fails -->
	{#each errors as error}
		{#if error}
			<p class="text-wrap text-xs text-destructive">{error}</p>
		{/if}
	{/each}
</div>
