using Planetbase;
using Redirection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BuildingAligner
{
    public class BuildingAligner : IMod
    {
        public static bool rendering = false;

        public void Init()
        {
            DebugRenderer.addGroup("Connections");
            Redirector.PerformRedirections();
            Debug.Log("[MOD] BuildingAligner activated");
        }

        public void Update()
        {
            GameStateGame gameState = GameManager.getInstance().getGameState() as GameStateGame;
            if (gameState != null && gameState.mMode != GameStateGame.Mode.PlacingModule)
            {
                rendering = false;
                DebugRenderer.clearGroup("Connections");
            }
        }
    }

    public abstract class CustomGameStateGame : GameStateGame
    {
        private CustomGameStateGame(string s, int i) : base(s, i) { }

        [RedirectFrom(typeof(GameStateGame))]
        public new void tryPlaceModule()
        {
            DebugRenderer.clearGroup("Connections");

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int layerMask = 256;
            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit, 150f, layerMask))
            {
                float size = Module.ValidSizes[mCurrentModuleSize];
                if (mActiveModule == null)
                {
                    mActiveModule = Module.create(raycastHit.point, mCurrentModuleSize, mPlacedModuleType);
                    mActiveModule.setRenderTop(mRenderTops);
                    mActiveModule.setValidPosition(false);
                    mCost = mActiveModule.calculateCost();
                }

                if (mCurrentModuleSize != mActiveModule.getSizeIndex())
                {
                    mActiveModule.changeSize(mCurrentModuleSize);
                    mCost = mActiveModule.calculateCost();
                    mActiveModule.setValidPosition(false);
                }

                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                {
                    raycastHit.point = RenderAvailablePositions(raycastHit.point);
                    //TryAlign(ref raycastHit);
                }
                else
                {
                    BuildingAligner.rendering = false;
                    DebugRenderer.clearGroup("Connections");
                }

                bool flag = mActiveModule.canPlaceModule(raycastHit.point, raycastHit.normal, size);
                if (inTutorial())
                {
                    snapToTutorialPosition(ref raycastHit, ref flag);
                }

                Vector3 point = raycastHit.point;
                float floorHeight = TerrainGenerator.getInstance().getFloorHeight();
                point.y = floorHeight;
                if (!mActiveModule.isValidPosition() || flag || (point - mActiveModule.getPosition()).magnitude > 5f)
                {
                    mActiveModule.setValidPosition(flag);
                    mActiveModule.setPosition(point);
                }
                mActiveModule.setPositionY(floorHeight + 0.1f);
            }
        }

        public Vector3 RenderAvailablePositions(Vector3 point)
        {
            float closestDist = float.MaxValue;
            Vector3 closestPos = point;
            float floorHeight = TerrainGenerator.getInstance().getFloorHeight();

            foreach (Module module in Module.mModules)
            {
                if (module == null || module == mActiveModule)
                    continue;

                Vector3 modulePos = module.getPosition();

                bool connectionAvailable = false;

                int count = 0;
                List<Vector3> positions = GetPositionsAroundModule(module);
                foreach (Vector3 position in positions)
                {
                    Vector3 p = position;
                    p.y = floorHeight;

                    float dist = Vector3.Distance(p, point);
                    if (dist < 35f && Connection.canLink(mActiveModule, module, p, modulePos) && canPlaceModule(p, Vector3.up, Module.ValidSizes[mCurrentModuleSize]))
                    {
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestPos = p;
                        }

                        connectionAvailable = true;
                    }

                    if (count == 4 && connectionAvailable)
                    {
                        DebugRenderer.addLine("Connections", modulePos + (p - modulePos).normalized * module.getRadius(), p, Color.blue, 0.5f);
                        DebugRenderer.addCube("Connections", p, Color.blue, 1.0f);
                        connectionAvailable = false;
                    }

                    count = ++count % 5;
                }
            }

            //hit.point = closestPos;
            BuildingAligner.rendering = true;
            return closestPos;
        }

        public bool canPlaceModule(Vector3 position, Vector3 normal, float radius)
        {
            float floorHeight = Singleton<TerrainGenerator>.getInstance().getFloorHeight();
            float heightDiff = position.y - floorHeight;

            bool isMine = mActiveModule.hasFlag(ModuleType.FlagMine);
            if (isMine)
            {
                if (heightDiff < 1f || heightDiff > 3f)
                {
                    // mine must be a little elevated
                    return false;
                }
            }
            else if (heightDiff > 0.1f || heightDiff < -0.1f)
            {
                // not at floor level
                return false;
            }

            // here we're approximating the circumference of the structure with 8 points
            // and will check that all these points are level with the floor
            float reducedRadius = radius * 0.75f;
            float angledReducedRadius = reducedRadius * 1.41421354f * 0.5f;
            Vector3[] circumference = new Vector3[]
            {
                position + new Vector3(reducedRadius, 0f, 0f),
                position + new Vector3(-reducedRadius, 0f, 0f),
                position + new Vector3(0f, 0f, reducedRadius),
                position + new Vector3(0f, 0f, -reducedRadius),
                position + new Vector3(angledReducedRadius, 0f, angledReducedRadius),
                position + new Vector3(angledReducedRadius, 0f, -angledReducedRadius),
                position + new Vector3(-angledReducedRadius, 0f, angledReducedRadius),
                position + new Vector3(-angledReducedRadius, 0f, -angledReducedRadius)
            };

            if (isMine)
            {
                // above we verified that it is a bit elevated
                // now make sure that at least one point is near level ground
                bool isValid = false;
                for (int i = 0; i < circumference.Length; i++)
                {
                    Vector3 floor;
                    PhysicsUtil.findFloor(circumference[i], out floor, 256);
                    if (floor.y < floorHeight + 1f || floor.y > floorHeight - 1f)
                    {
                        isValid = true;
                        break;
                    }
                }

                if (!isValid)
                {
                    return false;
                }
            }
            else
            {
                // Make sure all points are near level ground
                for (int j = 0; j < circumference.Length; j++)
                {
                    Vector3 floor;
                    PhysicsUtil.findFloor(circumference[j], out floor, 256);
                    if (floor.y > floorHeight + 2f || floor.y < floorHeight - 1f)
                    {
                        return false;
                    }
                }
            }

            //position.y = floorHeight;

            // Can only be 375 units away from center of map
            Vector2 mapCenter = new Vector2(TerrainGenerator.TotalSize, TerrainGenerator.TotalSize) * 0.5f;
            float distToCenter = (mapCenter - new Vector2(position.x, position.z)).magnitude;
            if (distToCenter > 375f)
            {
                return false;
            }

            // anyPotentialLinks limits connection to 20 (on top of some other less relevant checks)
            //if (Module.mConstructions.Count > 1 && !anyPotentialLinks(position))
            //{
            //    return false;
            //}

            RaycastHit[] array2 = Physics.SphereCastAll(position + Vector3.up * 20f, radius * 0.5f + 3f, Vector3.down, 40f, 4198400);
            if (array2 != null)
            {
                for (int k = 0; k < array2.Length; k++)
                {
                    RaycastHit raycastHit = array2[k];
                    GameObject gameObject = raycastHit.collider.gameObject.transform.root.gameObject;
                    Construction construction = Construction.mConstructionDictionary[gameObject];
                    if (construction != null)
                    {
                        //if (construction is Connection)
                        //{
                        //    return false;
                        //}
                        float distToConstruction = (position - construction.getPosition()).magnitude - mActiveModule.getRadius() - construction.getRadius();
                        if (distToConstruction < 3f)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Not hitting construction: " + gameObject.name);
                    }
                }
            }

            // Check that it's away from the ship
            if (Physics.CheckSphere(position, radius * 0.5f + 3f, 65536))
            {
                return false;
            }

            // Check that it doesn't overlap materials
            //if (Physics.CheckSphere(position, radius * 0.5f + 2f, 1024))
            //{
            //    return false;
            //}

            // This is to rotate the mine. We're setting the mine as auto-rotate instead
            //if (isMine)
            //{
            //    Vector3 vector3 = new Vector3(normal.x, 0f, normal.z);
            //    Vector3 normalized = vector3.normalized;
            //    if (Vector3.Dot(this.mObject.transform.forward, normalized) < 0.8660254f)
            //    {
            //        this.mObject.transform.forward = normalized;
            //    }
            //}

            return true;
        }

        public List<Vector3> GetPositionsAroundModule(Module module)
        {
            List<Vector3> positions = new List<Vector3>();

            Vector3 pos = module.getPosition();
            Vector3 dir = module.getTransform().forward;
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    positions.Add(pos + dir * (10f + 5f * j));
                }

                dir = Quaternion.Euler(0f, 30f, 0f) * dir;
            }

            return positions;
        }

        public void TryAlign(ref RaycastHit raycastHit)
        {
            // find available connections
            List<Module> linkableModules = new List<Module>();
            foreach (Module module in Module.mModules)
            {
                if (module == null || module == mActiveModule)
                    continue;

                if (Connection.canLink(mActiveModule, module, raycastHit.point, module.getPosition()))
                {
                    linkableModules.Add(module);
                }
            }

            // TODO: if more than 2, keep the closest ones

            //float closestDist1 = float.MaxValue;
            //float closestDist2 = float.MaxValue;
            //for (int i = 0; i < Module.mModules.Count; i++)
            //{
            //    Module module = Module.mModules[i];
            //    if (module != null && module != mActiveModule)
            //    {
            //        float dist = Vector3.Distance(module.getPosition(), raycastHit.point);
            //        if (dist < closestDist1)
            //        {
            //            closestDist2 = closestDist1;
            //            closestDist1 = dist;
            //            linkableModules.Insert(0, module);
            //            if (linkableModules.Count > 2)
            //                linkableModules.RemoveRange(2, linkableModules.Count - 2);
            //        }
            //        else if (dist < closestDist2)
            //        {
            //            closestDist2 = dist;
            //            linkableModules.Insert(1, module);
            //            if (linkableModules.Count > 2)
            //                linkableModules.RemoveRange(2, linkableModules.Count - 2);
            //        }
            //    }
            //}

            // try align with all
            if (linkableModules.Count == 1)
            {
                Vector3 closestPoint, closestDir;
                GetClosestPosAndDir(raycastHit.point, linkableModules[0], out closestPoint, out closestDir);

                float dist = Vector3.Distance(closestPoint, linkableModules[0].getPosition());
                float a = dist % 5f;
                if (a < 2.5f)
                    dist -= a;
                else
                    dist += 5f - a;

                closestPoint = linkableModules[0].getPosition() + closestDir * dist;

                raycastHit.point = closestPoint;
            }
            else if (linkableModules.Count > 1)
            {
                Vector3 point = raycastHit.point;

                float closestDist = float.MaxValue;
                Vector3 closestPos = raycastHit.point;

                Vector3 pos1 = linkableModules[0].getPosition();
                pos1.y = 0f;
                Vector3 pos2 = linkableModules[1].getPosition();
                pos2.y = 0f;

                Vector3 dir1 = linkableModules[0].getTransform().forward;
                dir1.y = 0f;
                Vector3 dir2 = linkableModules[1].getTransform().forward;
                dir2.y = 0f;

                point.y = 0f;
                for (int i = 0; i < 12; i++)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        if (Vector3.Dot(dir1, dir2) >= -0.9f && Vector3.Dot(dir1, dir2) <= 0.9f)
                        {
                            Vector3 intersection;
                            if (LineLineIntersection(out intersection, pos1, dir1, pos2, dir2))
                            {
                                float dist = Vector2.Distance(point, intersection);
                                if (dist < closestDist)
                                {
                                    closestDist = dist;
                                    closestPos = intersection;
                                }
                            }
                        }

                        dir2 = Quaternion.Euler(0f, 30f, 0f) * dir2;
                    }

                    dir1 = Quaternion.Euler(0f, 30f, 0f) * dir1;
                }

                float dist2 = Vector3.Distance(closestPos, linkableModules[0].getPosition());
                float a = dist2 % 5f;
                if (a < 2.5f)
                    dist2 -= a;
                else
                    dist2 += 5f - a;

                closestPos = linkableModules[0].getPosition() + (closestPos - linkableModules[0].getPosition()).normalized * dist2;

                raycastHit.point = new Vector3(closestPos.x, raycastHit.point.y, closestPos.z);
                mActiveModule.setPosition(raycastHit.point);
            }
        }

        public void GetClosestPosAndDir(Vector3 point, Module module, out Vector3 closestPoint, out Vector3 closestDir)
        {
            Vector3 pos = module.getTransform().position;
            Vector3 dir = module.getTransform().forward;

            closestPoint = pos;
            closestDir = dir;

            float closestDist = float.MaxValue;
            for (int i = 0; i < 12; i++)
            {
                float t = Vector3.Dot(point - pos, dir);
                Vector3 closestPos = pos + dir * t;

                float dist = Vector3.Distance(point, closestPos);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestPoint = closestPos;
                    closestDir = dir;
                }

                dir = Quaternion.Euler(0, 30f, 0) * dir;
            }
        }

        public bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
        {

            intersection = Vector3.zero;

            Vector3 lineVec3 = linePoint2 - linePoint1;
            Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
            Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

            float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

            //Lines are not coplanar. Take into account rounding errors.
            if ((planarFactor >= 0.00001f) || (planarFactor <= -0.00001f))
            {
                return false;
            }

            //Note: sqrMagnitude does x*x+y*y+z*z on the input vector.
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;

            //if ((s >= 0.0f) && (s <= 1.0f))
            {

                intersection = linePoint1 + (lineVec1 * s);
                return true;
            }

            //else
            //{
            //    return false;
            //}
        }

    }
}
