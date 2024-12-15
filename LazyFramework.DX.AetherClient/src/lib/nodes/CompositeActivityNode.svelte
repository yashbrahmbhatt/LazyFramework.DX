<script lang="ts" module>
	export interface CompositeActivityNodeProps {
		[key: string]: any;
		name: string;
		description: string;
		properties: Argument[];
		groups: Record<string, string>;
	}
</script>

<script lang="ts">
	import type { Activity, Argument } from '$lib/models/Solution';
	import {
		Handle,
		Position,
		type HandleProps,
		type Node,
		type NodeProps,
		type XYPosition
	} from '@xyflow/svelte';
	import * as Card from '$lib/components/ui/card';
	import ActivityNode from '$lib/nodes/ActivityNode.svelte';
	type $$Props = NodeProps;

	let { id, data, width, height }: $$Props & { data: CompositeActivityNodeProps } = $props();
</script>

<Card.Root class="m-4 p-4" {id}>
	<Card.Header>
		<Card.Title>{data.name}</Card.Title>
		<Card.Description>
			<div>
				{data.description}
			</div>
		</Card.Description>
	</Card.Header>
	<Card.Content >
		<div>
			{#each data.properties as prop}
				<div>
					<strong>{prop.Name}</strong>: {prop.DefaultValue}
				</div>
			{/each}
		</div>
		<div class='bg-muted' id={id + ".content"} style={`width: ${width + 30}px; height: ${height + 30}px;`}>
		</div>	
	</Card.Content>
</Card.Root>
