"""Before Us - reference balance simulation (rules v6).

Usage:  python before_us_sim.py        -> full game (win at stage 5)
        python before_us_sim.py mvp    -> first playable (win at stage 3, 2 regions)

This file is the numeric reference for docs/design.md. The C# Core must reproduce
its results (see docs/plan.md, Gate 2). Bot styles: balanced (thoughtful player),
safe (never takes optional combat), reckless (fights everything), careless (random).

Simplifications vs the real game: member speed/strength/HP are averaged; optional
neuron nodes are not bought; beast attack types are folded into one average
death chance on a won combat (win_deaths).
"""
import random, statistics, collections, math


P = dict(
    start_crew=5, energy_max=[4, 5, 6, 7], upkeep=[1, 1, 2, 3],
    required_cost=[85, 135, 190, 240],
    cost=dict(gather=1, morale=1, build=2, hunt=2, explore=3),
    neuron=dict(gather=1, morale=1, build=2, hunt=6, explore=6),
    gather_base=3, region_food=1, cooked_bonus=1, region_max=5,
    hunt_per_crew=[0, 5, 7, 10],
    hunt_p=0.50, explore_p=0.40, p_per_crew=0.08, p_per_build=0.05, p_cap=0.92,
    win_deaths=0.5, lose_deaths=2,   # win_deaths = average chance a won combat kills 1 member (real game: per beast attack type)
    quota=[1, 2, 2, 3, 3, 3],                      # members a raiding beast eats, by region
    morale_per_crew=2, morale_success=1, morale_hunt=3, morale_fail=-2, birth_cost=4,
    cap_base=6, cap_per_build=2, build_need=4,
    diff_period=20, diff_combat=0.02, diff_event=1, diff_quota=1, diff_gather=1, diff_raid=0.0, gather_floor=0,   # each difficulty level: combat odds -2 pts, raid eats +1, bad events -1 more, gather -1
    grace_turns=5, tutorial_turn=3, explore_from=6,
    phase_len=10, dry_penalty=[1, 1, 2, 2],  # gather loss per member in Dry, by stage
    event_p=0.7, ev_choice=0.4, ev_raid_wet=0.20, ev_raid_dry=0.25,
    raid_base=0.35, raid_per_crew=0.05, raid_per_build=0.06, raid_per_stage=0.04, raid_per_region=0.05,
    # feats needed to evolve from stage 1..4: (combat wins of any kind, hunts, migrated at least N times ever)
    feats=[(1, 0, 0), (0, 2, 0), (0, 2, 1), (0, 3, 0)],
    win_stage=5,
)

# First playable (vertical slice): stages 1-2 content, 2 regions, win on reaching stage 3.
P_MVP = dict(P, region_max=1, win_stage=3, required_cost=[150, 250, 190, 240], diff_period=12)

def clamp(x, a, b): return max(a, min(b, x))

def play(P, rng, style='balanced'):
    s, crew, neuron, morale, build, bpts, region = 1, P['start_crew'], 0, 0, 0, 0, 0
    wins = hunts = explores = 0; explored_ever = 0
    for turn in range(1, 151):
        dry = (turn - 1) // P['phase_len'] % 2 == 1
        lvl = turn // P['diff_period'] if P['diff_period'] else 0
        E = P['energy_max'][s - 1]; avail = crew; u = P['upkeep'][s - 1]
        cap = P['cap_base'] + P['cap_per_build'] * build
        gy = P['gather_base'] + P['region_food'] * region + (P['cooked_bonus'] if s >= 4 else 0)
        if dry: gy = max(1, gy - P['dry_penalty'][s - 1])
        gy = max(P['gather_floor'], gy - int(P['diff_gather'] * lvl))
        pf = lambda base, k: clamp(base - P['diff_combat'] * lvl + P['p_per_crew'] * k + P['p_per_build'] * build, 0.05, P['p_cap'])
        can = dict(gather=True, morale=True, build=s >= 2, hunt=s >= 2,
                   explore=turn >= P['explore_from'] and region < P['region_max'])
        need_w, need_h, need_x = P['feats'][s - 1] if s < 5 else (0, 0, 0)
        acts = []
        def add(a, k):
            nonlocal avail, E
            k = min(k, avail)
            if k > 0 and E >= P['cost'][a] and can[a]:
                acts.append((a, k)); avail -= k; E -= P['cost'][a]
        if style == 'careless':
            opts = [a for a in can if can[a]]
            while avail > 0 and E >= 1:
                a = rng.choice(opts)
                if P['cost'][a] > E: break
                add(a, rng.randint(1, avail))
        else:
            add('gather', math.ceil(crew * u / gy) if gy > 0 else avail)
            fight = style != 'safe'
            want_x = explored_ever < need_x or (dry and region < P['region_max']) or style == 'reckless'
            if fight and want_x and (crew >= 5 or style == 'reckless'): add('explore', 3)
            if crew >= cap - 1: add('build', 2)
            want_h = hunts < need_h or wins < need_w or (crew >= 7 and style != 'balanced') or style == 'reckless'
            if fight and want_h and (crew >= 3 or style == 'reckless'): add('hunt', min(3, crew - 1) if style != 'reckless' else 3)
            if crew < cap: add('morale', 2)
            add('gather', avail)
        food = 0
        for a, k in acts:
            neuron += P['neuron'][a] * k
            if a == 'gather': food += k * gy; morale += P['morale_success']
            elif a == 'morale': morale += P['morale_per_crew'] * k
            elif a == 'build':
                bpts += k; morale += P['morale_success']
                if bpts >= P['build_need'] * (build + 1): bpts = 0; build += 1
            else:
                if rng.random() < pf(P['hunt_p'] if a == 'hunt' else P['explore_p'], k):
                    morale += P['morale_hunt']; wins += 1; crew -= (1 if rng.random() < P['win_deaths'] else 0)
                    if a == 'hunt': food += k * P['hunt_per_crew'][s - 1]; hunts += 1
                    else: region = min(region + 1, P['region_max']); explores += 1; explored_ever += 1
                else:
                    morale += P['morale_fail']; crew -= P['lose_deaths']
        # computer turn
        if turn == P['tutorial_turn']:
            morale += 2; neuron += 3; wins += 1           # tutorial combat: easy, no deaths
        elif turn > P['grace_turns'] and rng.random() < P['event_p']:
            r = rng.random(); raid = (P['ev_raid_dry'] if dry else P['ev_raid_wet']) + P['diff_raid'] * lvl
            p = clamp(P['raid_base'] + P['raid_per_crew'] * crew + P['raid_per_build'] * build
                      + P['raid_per_stage'] * s - P['raid_per_region'] * region - P['diff_combat'] * lvl, 0.05, 0.95)
            if r < P['ev_choice']:
                good = rng.random() < (0.5 if style == 'careless' else 0.6)
                food += 4 if good else -(3 + P['diff_event'] * lvl); morale += 2 if good else -(2 + P['diff_event'] * lvl)
            elif r < P['ev_choice'] + raid:              # forced defence
                if rng.random() < p: morale += 2; neuron += 3; wins += 1; crew -= (1 if rng.random() < P['win_deaths'] else 0)
                else: crew -= min(crew, P['quota'][min(region, len(P['quota']) - 1)] + int(P['diff_quota'] * lvl))
            else:                                         # optional opportunity combat
                attack = style in ('careless', 'reckless') or (style == 'balanced' and p > 0.6 and crew >= 7)
                if attack:
                    if rng.random() < p: food += 6; morale += 3; neuron += 4; wins += 1; crew -= (1 if rng.random() < P['win_deaths'] else 0)
                    else: crew -= P['lose_deaths']
        if food < crew * u: crew -= 1
        morale = max(0, morale)
        if crew > 0 and morale >= P['birth_cost'] and crew < cap: crew += 1; morale -= P['birth_cost']
        if s < 5 and neuron >= P['required_cost'][s - 1] and wins >= need_w and hunts >= need_h and explored_ever >= need_x:
            neuron -= P['required_cost'][s - 1]; s += 1; wins = hunts = explores = 0
        if s == P['win_stage']: return True, turn, s
        if crew <= 0: return False, turn, s
    return False, 150, s

def run(P, style='balanced', n=3000, seed=21):
    rng = random.Random(seed); res = [play(P, rng, style) for _ in range(n)]
    w = [t for ok, t, _ in res if ok]
    q = statistics.quantiles(w, n=10) if len(w) > 10 else [0] * 9
    return dict(win=round(len(w) / n, 3), median=statistics.median(w) if w else None, p10=q[0], p90=q[-1],
                timeout=round(sum(1 for ok, t, _ in res if not ok and t == 150) / n, 3),
                losses_by_stage=dict(sorted(collections.Counter(st for ok, t, st in res if not ok).items())))

if __name__ == '__main__':
    import sys
    params = P_MVP if 'mvp' in sys.argv[1:] else P
    print('mode:', 'MVP (first playable)' if params is P_MVP else 'full game')
    for st in ('balanced', 'safe', 'reckless', 'careless'):
        print(f"{st:9s}", run(params, st))
