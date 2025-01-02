<script lang="ts" module>
	export interface SideBarLink {
		title: string;
		url: string;
		items?: SideBarLink[];
		icon?: (props: any) => ReturnType<Snippet>;
	}
</script>

<script lang="ts">
	import Button from '$lib/components/ui/button/button.svelte';
	import { Label } from '$lib/components/ui/label';
	import * as Sidebar from '$lib/components/ui/sidebar';
	import {
		Brain,
		Bug,
		ChevronDown,
		ChevronUp,
		GalleryVerticalEnd,
		Home,
		Logs,
		Settings
	} from 'lucide-svelte';
	import type { ComponentProps, Snippet, SvelteComponent } from 'svelte';
	import * as Collapsible from '$lib/components/ui/collapsible';
	import Separator from '$lib/components/ui/separator/separator.svelte';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	let {
		ref = $bindable(null),
		data,
		open,
		toggleLogs,
		...restProps
	}: ComponentProps<typeof Sidebar.Root> & {
		data: SideBarLink[];
		open: boolean;
		toggleLogs: () => void;
	} = $props();

	$inspect($page.url.pathname);
</script>

<Sidebar.Root variant="inset" {...restProps} collapsible="icon">
	<Sidebar.Header class="min-h-16 place-content-center">
		<Sidebar.Menu>
			<Sidebar.MenuItem>
				<Sidebar.MenuButton size="lg" onclick={() => goto('/')}>
					{#snippet child({ props })}
						<a href="/" {...props}>
							<div
								class="flex aspect-square size-8 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground"
							>
								<Brain class="size-4" />
							</div>
							<div class="flex flex-col gap-0.5 leading-none">
								<span class="text-lg font-bold">LazyFramework.DX</span>
							</div>
						</a>
					{/snippet}
				</Sidebar.MenuButton>
			</Sidebar.MenuItem>
		</Sidebar.Menu>
	</Sidebar.Header>
	<Separator class="bg-sidebar-foreground/30" />
	<Sidebar.Content>
		<Sidebar.Group>
			<Sidebar.Menu class="gap-2">
				{#each data as mainItem, index (mainItem.title)}
					<Collapsible.Root open={index === 1} class="group/collapsible">
						<Sidebar.MenuItem>
							<Collapsible.Trigger>
								{#snippet child({ props })}
									<Sidebar.MenuButton
										{...props}
										isActive={$page.url.pathname === mainItem.url}
										onclick={() => (mainItem.items?.length ? {} : goto(mainItem.url))}
									>
										{#if mainItem.icon}
											{@render mainItem.icon({ class: 'min-size-4' })}
										{/if}
										{mainItem.title}{' '}
										{#if mainItem.items?.length}
											<ChevronDown class="ml-auto group-data-[state=open]/collapsible:hidden" />
											<ChevronUp class="ml-auto group-data-[state=closed]/collapsible:hidden" />
										{/if}
									</Sidebar.MenuButton>
								{/snippet}
							</Collapsible.Trigger>
							{#if mainItem.items?.length}
								<Collapsible.Content>
									<Sidebar.MenuSub>
										{#each mainItem.items as item (item.title)}
											<Sidebar.MenuSubItem>
												<Sidebar.MenuSubButton isActive={$page.url.pathname === item.url}>
													{#snippet child({ props })}
														<a href={item.url} {...props}>{item.title}</a>
													{/snippet}
												</Sidebar.MenuSubButton>
											</Sidebar.MenuSubItem>
										{/each}
									</Sidebar.MenuSub>
								</Collapsible.Content>
							{/if}
						</Sidebar.MenuItem>
					</Collapsible.Root>
				{/each}
			</Sidebar.Menu>
		</Sidebar.Group>
	</Sidebar.Content>
	<Separator class="bg-sidebar-foreground/30" />
	<Sidebar.Footer>
		<Sidebar.Menu class="flex justify-evenly {open ? 'flex-row' : 'flex-col'}">
			<Sidebar.MenuItem>
				<Sidebar.MenuButton size="lg" class="">
					{#snippet child({ props })}
						<Button variant="ghost" {...props}><Settings class="min-size-4" /></Button>
					{/snippet}
				</Sidebar.MenuButton>
			</Sidebar.MenuItem>
			<Sidebar.MenuItem>
				<Sidebar.MenuButton size="lg" class="">
					{#snippet child({ props })}
						<Button variant="ghost" {...props}><Bug class="min-size-4" /></Button>
					{/snippet}
				</Sidebar.MenuButton>
			</Sidebar.MenuItem>
			<Sidebar.MenuItem>
				<Sidebar.MenuButton size="lg" class="">
					{#snippet child({ props })}
						<Button variant="ghost" {...props} onclick={toggleLogs}
							><Logs class="min-size-4" /></Button
						>
					{/snippet}
				</Sidebar.MenuButton>
			</Sidebar.MenuItem>
		</Sidebar.Menu>
	</Sidebar.Footer>
</Sidebar.Root>
