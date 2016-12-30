## LifeSim - Game of Life

LifeSim, which plays on console screen, is basic a simulation. It is carried out on 16X32 Array. There are 4 special particle that Q,W,and E are static, R is randomly. It has some rules, keys, and properties.
## Keys

```markdown
- Q : Particle-Q is located
- W : Particle-W is located
- E : Particle-E is located
- R : Particle-R is located
- T : Change particle-R randomly
- Y : Create random particle-R and locate it
- 0 : Clear all screen
- 1 : Rotate particle-Q
- 2 : Rotate particle-W
- 3 : Clear cell that cursor is on
- 4 : Rotate particle-R
- Enter : Continuously creating next generation
```
## Evolution Rules
1. Cell has two and three live cells, stays next generation live
2. Cell has not less than 2 live cells, becomes dead next generation due to underpopolation
3. Cell has more than 3 live cells, becomes dead next generation due to overcrowding