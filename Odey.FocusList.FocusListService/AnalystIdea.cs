using Odey.FocusList.Contracts;
using E=Odey.Framework.Keeley.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Odey.FocusList.FocusListService
{
    public class AnalystIdeaManager
    {

        public AnalystIdeaManager(E.KeeleyModel context)
        {
            _context = context;
        }
        //    public void Add(List<int issuerId, bool isLong, DateTime effectiveFromDate, DateTime effectiveToDate)
        //    {

        //    }

        private E.KeeleyModel _context;

        public void Add(AnalystIdea dto)
        {
            if (dto.FocusLists == null)
            {
                dto.FocusLists = _context.FocusLists.Where(a => a.IssuerId == dto.IssuerId).Select(a => new FocusListDTO()
                {
                    AnalystId = a.AnalystId,
                    EffectiveFromDate = a.InDate,
                    EffectiveToDate = a.OutDate ?? new DateTime(9999, 12, 31),
                    InPrice = a.InPrice,
                    InstrumentMarketId = a.InstrumentMarketId,
                    IsLong = a.IsLong
                }).ToList();
            }
            var newIdeas = ResolveNew(dto);
            var existingIdeas = _context.AnalystIdea2.Where(a => a.IssuerId == dto.IssuerId).ToList();
            Match(existingIdeas, newIdeas);
         
        }

        private void Match(List<E.AnalystIdea2> existingIdeas, List<E.AnalystIdea2> newIdeas)
        {
            Dictionary<E.AnalystIdea2, E.AnalystIdea2> matchedIdeas = new Dictionary<E.AnalystIdea2, E.AnalystIdea2>();
            List<E.AnalystIdea2> ideasToAdd = new List<E.AnalystIdea2>();
            foreach (var newIdea in newIdeas)
            {
                var exists = existingIdeas.FirstOrDefault(a => a.IsLong = newIdea.IsLong && a.EffectiveFromDate == newIdea.EffectiveFromDate && a.FocusListId == newIdea.FocusListId && a.AnalystId == newIdea.AnalystId && a.OriginatiorId == newIdea.OriginatiorId);
                if (exists != null)
                {
                    matchedIdeas.Add(exists,newIdea);
                }
                else
                {
                    ideasToAdd.Add(newIdea);
                }
            }
            List<E.AnalystIdea2> ideasToRemove = new List<E.AnalystIdea2>();
            foreach(var existingIdea in existingIdeas)
            {
                if (!matchedIdeas.ContainsKey(existingIdea))
                {
                    ideasToRemove.Add(existingIdea);
                }
            }

            foreach(var matched in matchedIdeas)
            {
                var existing = matched.Key;
                var toApply = matched.Value;
                existing.Analyst = toApply.Analyst;
                existing.EffectiveToDate = toApply.EffectiveToDate;
                existing.FocusList = toApply.FocusList;
                existing.Originator = toApply.Originator;                
            }
            foreach(var ideaToAdd in ideasToAdd)
            {
                _context.AnalystIdea2.Add(ideaToAdd);
            }
            foreach (var ideaToRemove in ideasToRemove)
            {
                _context.AnalystIdea2.Remove(ideaToRemove);
            }
        }

        private List<E.AnalystIdea2> ResolveNew(AnalystIdea dto)
        {

            var dtos = dto.FocusLists.Concat<IAnalystIdea>(dto.Analysts).Concat<IAnalystIdea>(dto.Originators);
            var longs = ResolveForLongShort(dto, dtos, true);
            var shorts = ResolveForLongShort(dto, dtos, false);
            return longs.Concat(shorts).ToList();

        }

        private static readonly DateTime EOT = new DateTime(9999, 12, 31);

        private List<E.AnalystIdea2> ResolveForLongShort(AnalystIdea dto,IEnumerable<IAnalystIdea> dtos, bool isLong)
        {
            dtos = dtos.Where(a => a.IsLong == isLong).ToList();
            List<DateTime> datePoints = new List<DateTime>();
            bool createEOT = false;
            foreach (var newDTO in dtos.OrderBy(a => a.EffectiveFromDate).ThenBy(a => a.EffectiveToDate ?? EOT))
            {
                AddDatePoint(datePoints, newDTO.EffectiveFromDate);

                if (newDTO.EffectiveToDate.HasValue)
                {
                    AddDatePoint(datePoints, newDTO.EffectiveToDate.Value);
                }
                else
                {
                    createEOT = true;
                }
            }

            List<E.AnalystIdea2> analystIdeas = new List<E.AnalystIdea2>();
            DateTime? previous = null;
            foreach (DateTime datePoint in datePoints.OrderBy(a=>a))
            {
                if (previous != null)
                {
                    analystIdeas.Add(new E.AnalystIdea2() { IssuerId = dto.IssuerId,  EffectiveFromDate = previous.Value, EffectiveToDate = datePoint.AddDays(-1), IsLong = isLong });
                }
                previous = datePoint;
            }
            if (createEOT)
            {
                analystIdeas.Add(new E.AnalystIdea2() { IssuerId = dto.IssuerId, EffectiveFromDate = previous.Value, EffectiveToDate = null, IsLong = isLong });
            }
            foreach (var analystIdea in analystIdeas)
            {
                SetOriginator(analystIdea, dto);                
                SetFocusList(analystIdea, dto);
                SetAnalyst(analystIdea, dto);
            }
            return analystIdeas;
        }

        private Dictionary<Tuple<int?,int?,int?>, E.Originator> _originators = null;
        private Dictionary<Tuple<int?, int?, int?>, E.Originator> Originators
        {
            get
            {
                if (_originators == null)
                {
                    _originators = _context.Originators.ToList().ToDictionary(a=>GetOriginatorKey(a.InternalOriginatorId, a.ExternalOriginatorId, a.InternalOriginatorId2),a=>a);
                }
                return _originators;
            }
        }
        private Tuple<int?, int?, int?> GetOriginatorKey(int? internalOriginatorId, int? externalOriginatorId, int? internalOriginatorId2)
        {
            return new Tuple<int?, int?, int?>(internalOriginatorId, externalOriginatorId, internalOriginatorId2);
        }

        private void SetOriginator(E.AnalystIdea2 analystIdea, AnalystIdea dto)
        {
            analystIdea.Originator = GetOriginator(analystIdea, dto);
            if (analystIdea.Originator!=null && analystIdea.Originator.OriginatorId != 0)
            {
                analystIdea.OriginatiorId = analystIdea.Originator.OriginatorId;
            }
        }

        private E.Originator GetOriginator(E.AnalystIdea2 analystIdea, AnalystIdea dto)
        {
            OriginatorDTO originatorDTO = (OriginatorDTO)GetOverlapping(analystIdea, dto.Originators, analystIdea.IsLong);

            if (originatorDTO != null)
            {
                E.Originator originator;
                var key = GetOriginatorKey(originatorDTO.InternalOriginatorId, originatorDTO.ExternalOriginatorId, originatorDTO.InternalOriginatorId2);
                if (!Originators.TryGetValue(key, out originator))
                {
                    originator = new E.Originator() { ExternalOriginatorId = originatorDTO.ExternalOriginatorId, InternalOriginatorId = originatorDTO.InternalOriginatorId, InternalOriginatorId2 = originatorDTO.InternalOriginatorId2 };
                    Originators.Add(key, originator);
                    _context.Originators.Add(originator);
                }
                return originator;
            }
            return null;
        }

        private void SetAnalyst(E.AnalystIdea2 analystIdea, AnalystIdea dto)
        {
            analystIdea.Analyst = GetAnalyst(analystIdea, dto);
            if (analystIdea.Analyst != null && analystIdea.Analyst.UserID != 0)
            {
                analystIdea.AnalystId = analystIdea.Analyst.UserID;
            }
        }

        private E.ApplicationUser GetAnalyst(E.AnalystIdea2 analystIdea, AnalystIdea dto)
        {
            AnalystDTO analystDTO = (AnalystDTO)GetOverlapping(analystIdea, dto.Analysts, analystIdea.IsLong);

            if (analystDTO != null)
            {                
                var analyst = _context.ApplicationUsers.FirstOrDefault(a => a.UserID == analystDTO.AnalystId);
                if (analyst == null)
                {
                    throw new ApplicationException("Cant find Application User");
                }
                return analyst;
            }
            return null;
        }

        private void SetFocusList(E.AnalystIdea2 analystIdea, AnalystIdea dto)
        {
            analystIdea.FocusList = GetFocusList(analystIdea, dto);
            if (analystIdea.FocusList != null && analystIdea.FocusList.FocusListId != 0)
            {
                analystIdea.FocusListId = analystIdea.FocusList.FocusListId;
            }
        }

        private E.FocusList GetFocusList(E.AnalystIdea2 analystIdea, AnalystIdea dto)
        {
            FocusListDTO focusListDTO = (FocusListDTO)GetOverlapping(analystIdea, dto.FocusLists, analystIdea.IsLong);

            if (focusListDTO != null)
            {
                var focusList = _context.FocusLists.FirstOrDefault(a => a.InstrumentMarketId ==  focusListDTO.InstrumentMarketId && a.InDate == focusListDTO.EffectiveFromDate && a.AnalystId == focusListDTO.AnalystId );
                if (focusList == null)
                {
                    throw new ApplicationException("Cant find focus list");
                }
                return focusList;
            }
            return null;
        }

        private IAnalystIdea GetOverlapping(E.AnalystIdea2 analystIdea, IEnumerable<IAnalystIdea> inputs,bool isLong)
        {
            var overlapping = inputs.Where(a => a.EffectiveFromDate <= analystIdea.EffectiveFromDate && (analystIdea.EffectiveToDate ?? EOT) <= (a.EffectiveToDate ?? EOT) && a.IsLong == isLong).ToList();
            if (overlapping.Count==0)
            {
                return null;
            }
            if (overlapping.Count>1)
            {
                throw new ApplicationException("Too many inputs received for overlapping");
            }
            return overlapping.First();
        }

        private void AddDatePoint(List<DateTime> datePoints, DateTime dateToAdd)
        {
            if (!datePoints.Contains(dateToAdd))
            {
                datePoints.Add(dateToAdd);
            }
        }

    }
}