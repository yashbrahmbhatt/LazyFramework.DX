<script lang="ts">
	import * as Menubar from '$lib/components/ui/menubar';
	let { toggleLog }: { toggleLog: () => void } = $props();
</script>

<Menubar.Root>
	<Menubar.Menu>
		<Menubar.Trigger class="pl-4 font-mono text-sm"><a href="/">Home</a></Menubar.Trigger>
	</Menubar.Menu>
	<Menubar.Menu>
		<Menubar.Trigger>Solution</Menubar.Trigger>
		<Menubar.Content class="font-mono">
			<Menubar.Item href="/solution">Go to Solution</Menubar.Item>
			<Menubar.Separator />
			<Menubar.Item>Add Module</Menubar.Item>
			<Menubar.Item>Remove Module</Menubar.Item>
		</Menubar.Content>
	</Menubar.Menu>
	<Menubar.Menu>
		<Menubar.Trigger><a href="/configurations">Configurations</a></Menubar.Trigger>
	</Menubar.Menu>
	<div class="flex-grow"></div>
	<Menubar.Menu>
		<Menubar.Trigger class="pr-4 font-mono text-sm" on:click={toggleLog}>Logs</Menubar.Trigger>
	</Menubar.Menu>
</Menubar.Root>
